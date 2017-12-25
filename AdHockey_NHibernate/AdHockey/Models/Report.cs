/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Data;
using System.Configuration;
using System.Threading;
using Oracle.DataAccess.Client;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

using AdHockey.Repositories;

namespace AdHockey.Models {

    /// <summary>
    /// Export type. Excel or Access. 
    /// </summary>
    public enum ExportType {
        Excel, 
        AccessDB
    };

    /// <summary>
    /// Report is all fields associated with executing a sql report against the database. 
    /// </summary>
    [Bind(Include = "ReportName,Description,Sql")]
    public class Report {

        /// <summary>
        /// Unique report Id. 
        /// </summary>
        public int ReportId {
            get; set;
        }
        
        /// <summary>
        /// The name of the report, displayed by the GUI. 
        /// </summary>
        public String ReportName {
            get; set;
        }

        /// <summary>
        /// The description of the report displayed by the GUI. (make a tool tip)
        /// </summary>
        public String Description {
            get; set;
        }

        private String sql;

        /// <summary>
        /// The sql to be executed by the report, decorated by toggle comments. Toggle into code. 
        /// </summary>
        public String Sql {
            get {
                return sql;
            }
            set {
                Regex reg = new Regex("\\s*[;]\\s*$");
                if (reg.Match(value).Success)
                    sql = reg.Replace(value, "");
                else
                    sql = value;
            }
        }//end property

        /// <summary>
        /// Exports to a certain type, either accessdb or excel. 
        /// </summary>
        public ExportType ExportType {
            get; set;
        }//end property

        /// <summary>
        /// LimiterItems are placeholders for boolean values, check boxes. 
        /// </summary>
        public IList<LimiterItem> LimiterItems {
            get; set;
        }
        
        /// <summary>
        /// BulkTemplates are placeholders for file uploads which will be dumped into bulk tables to be used in the sql query. 
        /// </summary>
        public IList<BulkTemplate> BulkTemplates {
            get; set;
        }

        /// <summary>
        /// TemplateItems are placeholders for any type of input data which can use any html input control. 
        /// </summary>
        public IList<TemplateItem> TemplateItems {
            get; set;
        }

        /// <summary>
        /// Users that are subscribed to this report. 
        /// </summary>
        public IList<User> Users {
            get; set;
        }

        /// <summary>
        /// Groups that are subscribed to this report. 
        /// </summary>
        public IList<Group> Groups {
            get; set;
        }

        public IList<Synopsis> Synopses {
            get; set;
        }

        public Report() { }

        /// <summary>
        /// Gets the connection string used to connect to the database. 
        /// </summary>
        /// <returns></returns>
        private String GetConnectionString() {
            String connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
            return connStr;
        }

        /// <summary>
        /// Executes the report, toggles limitations into actual code, and applies TemplateItems to the SQL. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public DataTable ExecuteReport(int userId, String sessionId) {
            DataTable data = new DataTable();

            OracleConnection conn = null;
            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    String generatedSql = Sql;

                    generatedSql = ToggleLimitations(generatedSql, LimiterItems.ToList());
                    generatedSql = ToggleBulkValues(generatedSql, BulkTemplates.ToList(), userId, sessionId);

                    bool isAuthorized = AnyAuthorized(generatedSql, userId);

                    if (!isAuthorized) {
                        throw new UnauthorizedAccessException("User does not have authorization to some of the database objects in the query. ");
                    }

                    //for every User item set the arguments to the databse
                    foreach (var item in TemplateItems) {
                        cmd.Parameters.Add(item.CreateOracleParameter());
                    }//end loop

                    cmd.CommandText = generatedSql;
                    cmd.CommandType = CommandType.Text;

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd)) {
                        data = new DataTable();
                        adapter.Fill(data);
                    }
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }

            return data;
        }//end method

        /// <summary>
        /// Determine whether any group user is associated with is authorized for the data. 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AnyAuthorized(String sql, int userId) {
            List<Group> groups = null;
            try {
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    groups = grpRepo.GetAllGroups(userId);
                }
            }
            catch (Exception ex) {
                throw;
            }

            //loop over groups
            bool isAuthorized = false;
            foreach (var group in groups) {
                isAuthorized |= group.IsAuthorized(sql);
            }

            return isAuthorized;
        }//end method

        /// <summary>
        /// Toggles on and off limitations based on GUI input. 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public String ToggleLimitations(String sql, List<LimiterItem> items) {
            String retVal = sql;

            //get all matches
            Regex regNames = new Regex("(?<=[\\-]{2}LIMITATION\\s+)[^\\s]+");
            List<Match> matchNames = FindAllMatches(sql, regNames);

            Regex regLines = new Regex("[\\-]{2}LIMITATION\\s+.+\\s+[\"].*[\"]");
            List<Match> matchLines = FindAllMatches(sql, regLines);

            Regex regFirstPart = new Regex("[\\-]{2}LIMITATION\\s+[^\\s]+\\s+");
            List<Match> matchFirstPart = FindAllMatches(sql, regFirstPart);

            foreach (var item in items) {
                //find the match
                var theMatch = (from Match m in matchNames
                                where m.Value == item.TemplateName
                                select m).First();
                int index = matchNames.IndexOf(theMatch);

                String firstPart = matchFirstPart.ElementAt(index).Value;
                String wholeLine = matchLines.ElementAt(index).Value;
                int wholeLineIndex = matchLines.ElementAt(index).Index;

                //replace entire line with this value
                String value = retVal.Substring(wholeLineIndex + firstPart.Length, wholeLine.Length - firstPart.Length);

                //toggle on/off
                if (item.BoolVal == true)
                    retVal = retVal.Replace(wholeLine, RemoveQuotes(value));
            }

            return retVal;
        }//end method

        /// <summary>
        /// Remove quotes from the beginning and end of the string. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public String RemoveQuotes(String str) {
            char[] quotes = new char[] { '\"', ' '};
            return str.TrimStart(quotes).TrimEnd(quotes);
        }//end method

        /// <summary>
        /// Toggle the bulk value code if the user uploads a bulk value excel file. 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="items"></param>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public String ToggleBulkValues(String sql, List<BulkTemplate> items, int userId, String sessionId) {
            String retVal = sql;

            //get all matches
            Regex regNames = new Regex("(?<=[\\-]{2}BULKTEMPLATE\\s+)[^\\s]+");
            List<Match> matchNames = FindAllMatches(sql, regNames);

            Regex regLines = new Regex("[\\-]{2}BULKTEMPLATE\\s+.+\\s+[\"].*[\"]");
            List<Match> matchLines = FindAllMatches(sql, regLines);

            Regex regFirstPart = new Regex("[\\-]{2}BULKTEMPLATE\\s+[^\\s]+\\s+");
            List<Match> matchFirstPart = FindAllMatches(sql, regFirstPart);

            foreach (var item in items) {
                String appendStr = String.Format("LEFT JOIN BULK_ITEMS_STRING BI ON BI.USER_ID = {0} " 
                    + "\r\n AND BI.SESSION_ID = '{1}'" 
                    + "\r\n AND BI.TEMPLATE_ID = {2}" 
                    + "\r\n AND ", userId, sessionId, item.TemplateId);

                //find the match
                var theMatch = (from Match m in matchNames
                                where m.Value == item.TemplateName
                                select m).First();
                int index = matchNames.IndexOf(theMatch);

                String firstPart = matchFirstPart.ElementAt(index).Value;
                String wholeLine = matchLines.ElementAt(index).Value;
                int wholeLineIndex = matchLines.ElementAt(index).Index;

                //replace entire line with this value
                String value = retVal.Substring(wholeLineIndex + firstPart.Length, wholeLine.Length - firstPart.Length);

                //toggle on/off
                retVal = retVal.Replace(wholeLine, appendStr + RemoveQuotes(value));
            }

            return retVal;
        }//end method

        /// <summary>
        /// Finds all regular expression matches in a string. 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        private static List<Match> FindAllMatches(String str, Regex reg) {
            List<Match> matches = new List<Match>();
            int index = 0;
            Match match = null;

            do {
                //find names
                match = reg.Match(str, index);

                //add to list
                if (match.Success)
                    matches.Add(match);

                //iterate index keep moving forward
                index = match.Index + 1;
            } while (match.Success);

            return matches;
        }//end method

    }//end class

}//end namespace