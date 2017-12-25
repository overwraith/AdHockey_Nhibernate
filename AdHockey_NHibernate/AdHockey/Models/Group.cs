using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AdHockey.Models;
using Oracle.DataAccess.Client;
using System.Text.RegularExpressions;
using System.Data;
using System.Configuration;
using System.Diagnostics;

using AdHockey.Utilities;

namespace AdHockey.Models {
    public class Group {

        /// <summary>
        /// Unique identifier for group used by program and database layer. 
        /// </summary>
        public int GroupId {
            get; set;
        }

        /// <summary>
        /// Short group name for describing the group summarily. 
        /// </summary>
        public String GroupName {
            get; set;
        }

        /// <summary>
        /// Group description field describes the group. 
        /// </summary>
        public String Description {
            get; set;
        }

        /// <summary>
        /// All users associated with a given group. 
        /// </summary>
        public IList<User> Users {
            get; set;
        }

        /// <summary>
        /// All reports associated with a given group. 
        /// </summary>
        public IList<Report> Reports {
            get; set;
        }

        public IList<RestrictedField> RestrictedFields {
            get; set;
        }

        public IList<RestrictedTable> RestrictedTables {
            get; set;
        }

        public IList<RestrictedSchema> RestrictedSchemas {
            get; set;
        }

        //public IList<AuthorizedField> AuthorizedFields {
        //    get; set;
        //}

        public IList<AuthorizedTable> AuthorizedTables {
            get; set;
        }

        public IList<AuthorizedSchema> AuthorizedSchemas {
            get; set;
        }

        public Group() {

        }

        public bool HasAccess(Report report) {
            foreach (var grp in report.Groups)
                foreach(var usr in grp.Users)
                    if(grp.Users.Contains(usr))
                        return true;

            return false;
        }//end method

        /// <summary>
        /// Determine whether all black list options are authorized. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool IsAuthorized(String sql) {
            bool isAuthorized = true;

            //whitelists and black lists are mutually exclusive
            if (RestrictedFields.Count > 0 || RestrictedTables.Count > 0 || RestrictedSchemas.Count > 0) {
                //determine if is authorized
                foreach (var field in RestrictedFields)
                    isAuthorized &= field.IsAuthorized(sql);

                foreach (var table in RestrictedTables)
                    isAuthorized &= table.IsAuthorized(sql);

                foreach (var schema in RestrictedSchemas)
                    isAuthorized &= schema.IsAuthorized(sql);

                return isAuthorized;
            }

            if (AuthorizedTables.Count > 0 || AuthorizedSchemas.Count > 0) {//AuthorizedFields.Count > 0 
                //determine if is authorized
                //foreach (var field in AuthorizedFields)
                //    isAuthorized &= IsFieldAuthorized(sql);

                foreach (var table in AuthorizedTables)
                    isAuthorized &= IsTableAuthorized(sql);

                foreach (var schema in AuthorizedSchemas) {
                    isAuthorized &= IsSchemaAuthorized(sql);
                    Debug.WriteLine("isAuthorized: " + isAuthorized);
                }

                return isAuthorized;
            }

            return isAuthorized;
        }//end method

        /// <summary>
        /// Determines whether sql is authorized to execute. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool IsSchemaAuthorized(String sql) {
            String upperSql = sql.ToUpper();
            //have to match the most values first regex
            String reg = @"(FROM\s+[^\.\s\r\n]+[\.][^\.\s\r\n]+)" 
                        + @"|(JOIN\s+[^\.\s\r\n]+[\.][^\.]\s\r\n)"
                        + @"|(FROM\s+[^\.\s\r\n]+)"
                        + @"|(JOIN\s+[^\.\s\r\n]+)";
            Regex tableRegex = new Regex(reg);
            var matches = FindAllMatches(upperSql, tableRegex);

            //inflate ones that don't specify default schema
            List<String> schemaNames = new List<String>();
            foreach (var match in matches) {
                var str = match.Value;
                if (str.Contains('.')) {
                    //Debug.WriteLine("Substring: " + str.Substring(5) );
                    //Debug.WriteLine("Substring: " + str.Substring(5).Substring(0, str.Substring(5).IndexOf('.')));
                    schemaNames.Add(str.Substring(5).Substring(0, str.Substring(5).IndexOf('.')));
                }
                else {
                    //get default schema name
                    var schemaName = this.GetDefaultSchema();
                    schemaNames.Add(schemaName);
                }
            }//end loop

            //determine if all schemas are allowed
            bool isAuthorized = true;
            foreach (var schemaName in schemaNames) {
                //Debug.WriteLine("IsAuthorized: " + AuthorizedSchemas.Select(schema => schema.SchemaName).Contains(schemaName));
                isAuthorized &= AuthorizedSchemas.Select(schema => schema.SchemaName).Contains(schemaName);
            }//end loop

            return isAuthorized;
        }//end method

        /// <summary>
        /// Determine whether the sql is authorized to execute. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool IsTableAuthorized(String sql) {
            String upperSql = sql.ToUpper();
            //have to match the most values first regex
            String reg = @"(FROM\s+[^\.\s\r\n]+[\.][^\.\s\r\n]+)"
                        + @"|(JOIN\s+[^\.\s\r\n]+[\.][^\.]\s\r\n)"
                        + @"|(FROM\s+[^\.\s\r\n]+)"
                        + @"|(JOIN\s+[^\.\s\r\n]+)";
            Regex schemaRegex = new Regex(reg);
            var matches = FindAllMatches(upperSql, schemaRegex);

            //inflate ones that don't specify default schema
            List<AuthorizedTable> notAuthTabs = new List<AuthorizedTable>();
            foreach (var match in matches) {
                var str = match.Value;
                if (str.Contains('.')) {
                    Debug.WriteLine("Substring: " + str.Substring(5) );
                    Debug.WriteLine("Substring: " + str.Substring(5).Substring(0, str.Substring(5).IndexOf('.')));
                    AuthorizedTable authTab = new AuthorizedTable();
                    authTab.SchemaName = str.Substring(5).Substring(0, str.Substring(5).IndexOf('.'));

                    String otherReg = @"((?<=FROM\s+[^\.\s\r\n]+[\.])[^\.\s\r\n]+)"
                                + @"|((?<=JOIN\s+[^\.\s\r\n]+[\.])[^\.]\s\r\n)"
                                + @"|((?<=FROM\s+)[^\.\s\r\n]+)"
                                + @"|((?<=JOIN\s+)[^\.\s\r\n]+)";

                    Regex tableRegex = new Regex(otherReg);
                    authTab.TableName = tableRegex.Match(str, 0).Value;
                }
                else {
                    //get default schema name
                    var schemaName = this.GetDefaultSchema();
                    String otherReg = @"((?<=FROM\s+)[^\.\s\r\n]+)"
                                    + @"|((?<=JOIN\s+)[^\.\s\r\n]+)";
                    Regex tableRegex = new Regex(otherReg);

                    AuthorizedTable authTab = new AuthorizedTable();
                    authTab.SchemaName = this.GetDefaultSchema();
                    authTab.TableName = tableRegex.Match(str, 0).Value;

                    notAuthTabs.Add(authTab);
                }
            }//end loop

            //determine if all schemas are allowed
            bool isAuthorized = true;
            foreach (var unauthTab in notAuthTabs) {
                List<AuthorizedTable> authTabs = AuthorizedTables.ToList();
                authTabs.AddRange(DefaultAuthorizedTables());

                Debug.WriteLine("IsAuthorized: " + authTabs.Contains(unauthTab));
                isAuthorized &= authTabs.Contains(unauthTab);
            }//end loop

            return isAuthorized;
        }//end method

        //public bool IsFieldAuthorized(String sql) {
        //    //dictionary look up aliases to tables
        //    String upperSql = sql.ToUpper();
        //    bool isAuthorized = true;

        //    String regSelectsStr = @"(?<=SELECT[\s\r\n]+)([^\s\r\n,]+[\s\r\n]*[,]+[\s\r\n]*)+[^\s\r\n,]+(?=[\s\r\n]+FROM)";
        //    Regex regSelects = new Regex(regSelectsStr);
        //    List<String> selectStatements = new List<String>();

        //    FindAllMatches(upperSql, regSelects).ForEach((match) => selectStatements.Add(match.Value));

        //    //get tables and aliases
        //    Dictionary<String, AuthorizedTable> tableAliases = GetTableAliases(upperSql);

        //    //loop over select clauses
        //    foreach (var list in selectStatements) {
        //        for(int i = 0; i < list.Length;){
        //            //tokenize variables
        //            Regex reg = new Regex(@"[^\s\r\n,]+[\s\r\n]*[,]*[\s\r\n]*");
        //            Match match = reg.Match(list, i);

        //            if (!match.Success)
        //                break;

        //            var str = match.Value.TrimEnd().TrimEnd(',').TrimEnd();

        //            //can be "alias.colname"
        //            if (str.Contains(".")) {
        //                String colName = str.Substring(str.IndexOf(".") + 1);
        //                String key = str.Substring(0, str.IndexOf("."));
        //                AuthorizedTable nonAuthTab = tableAliases[key];
        //                AuthorizedField nonAuthField = new AuthorizedField();

        //                nonAuthField.ColumnName = colName;
        //                nonAuthField.SchemaName = nonAuthTab.SchemaName;
        //                nonAuthField.TableName = nonAuthTab.TableName;

        //                List<AuthorizedField> authFields = AuthorizedFields.ToList();
        //                //Debug.WriteLine("IsAuthorized: " + authFields.Contains(nonAuthField));
        //                isAuthorized &= authFields.Contains(nonAuthField);
        //            }
        //            else {
        //                //Regex tableReg = new Regex(@"((?<=FROM[\s\r\n]+[^\.\s\r\n]+[\.])[^\.\s\r\n]+)"
        //                //                            + @"|((?<=JOIN[\s\r\n]+[^\.\s\r\n]+[\.])[^\.\s\r\n]+)"
        //                //                            + @"|((?<=FROM[\s\r\n]+)[^\.\s\r\n]+)"
        //                //                            + @"|((?<=JOIN[\s\r\n]+)[^\.\s\r\n]+)");

        //                //List<String> tableNames = new List<String>();
        //                //FindAllMatches(upperSql, tableReg).ForEach(mat => tableNames.Add(mat.Value));

        //                ////can be "colname"
        //                //List<List<String>> inputList = new List<List<String>>();
        //                //inputList.Add(new List<String>());
        //                //inputList.Add(new List<String>());
        //                //inputList.Add(new List<String>());

        //                //inputList[0].Add(str);
        //                //tableAliases.Values.ToList().ForEach((table) => inputList[1].Add(table.TableName));
        //                //tableAliases.Values.ToList().ForEach((table) => inputList[2].Add(table.SchemaName));

        //                //List<List<String>> cartesian = CartesianProductTool.CartesianProduct(inputList);


        //                //AuthorizedField nonAuthField = ;
        //                //AuthorizedFields.Contains(new AuthorizedField() { ColumnName = str, SchemaName = , TableName = })
        //                throw new NotImplementedException("Please alias your sql columns Error recognizing table. ");
        //            }

        //            i = match.Index + match.Length;
        //        }
        //    }//end method

        //    return isAuthorized;
        //}//end method

        private Dictionary<String, AuthorizedTable> GetTableAliases(String sql) {

            String stopWords = "(LEFT)|(OUTER)|(JOIN)|(INNER)|(PIVOT)|(WHERE)";
            String reg = String.Format(@"(FROM[\s\r\n]+[^\.\s\r\n]+[\.][^\.\s\r\n]+[\s\r\n]+[^\s\r\n]+(?=[\s\r\n]+{0}))"
                                      + @"|(JOIN[\s\r\n]+[^\.\s\r\n]+[\.][^\.\s\r\n]+[\s\r\n]+[^\s\r\n]+(?=[\s\r\n]+{0}))"
                                      + @"|(FROM[\s\r\n]+[^\.\s\r\n]+[\.][^\.\s\r\n]+(?=[\s\r\n]+{0}))"
                                      + @"|(JOIN[\s\r\n]+[^\.\s\r\n]+[\.][^\.\s\r\n]+(?=[\s\r\n]+{0}))"
                                      + @"|(FROM[\s\r\n]+[^\.\s\r\n]+(?=[\s\r\n]+{0}))"
                                      + @"|(JOIN[\s\r\n]+[^\.\s\r\n]+(?=[\s\r\n]+{0}))", stopWords);
            Dictionary<String, AuthorizedTable> tableAliases = new Dictionary<String, AuthorizedTable>();
            List<Match> tabAlias = FindAllMatches(sql, new Regex(reg));

            //get table aliases
            foreach (var match in tabAlias) {
                var str = match.Value.Substring(5).Trim();
                if (str.Contains('.')) {
                    AuthorizedTable authTab = new AuthorizedTable();
                    String alias = "";
                    if (str.Contains(" ")) {
                        //could be "schema.table alias"
                        authTab.TableName = str.Substring(str.IndexOf(".") + 1, str.IndexOf(" ") - str.IndexOf(".") - 1);
                        authTab.SchemaName = str.Substring(0, str.IndexOf("."));
                        alias = str.Substring(str.IndexOf(" ") + 1);

                        tableAliases.Add(alias, authTab);

                        //add an aditional alias cover all bases
                        alias = String.Format("{0}.{1}", authTab.SchemaName, authTab.TableName).GetHashCode().ToString();

                        if(!tableAliases.ContainsKey(alias))
                            tableAliases.Add(alias, authTab);
                    }
                    else {
                        //could be "schema.table"
                        authTab.TableName = str.Substring(str.IndexOf(".") + 1);
                        authTab.SchemaName = str.Substring(0, str.IndexOf("."));
                        alias = String.Format("{0}.{1}", authTab.SchemaName, authTab.TableName).GetHashCode().ToString();
                        tableAliases.Add(alias, authTab);
                    }
                }
                else {
                    AuthorizedTable authTab = new AuthorizedTable();
                    String alias = "";
                    //get default schema
                    if (str.Contains(" ")) {
                        //could be "table alias"
                        authTab.TableName = str.Substring(0, str.IndexOf(' '));
                        authTab.SchemaName = this.GetDefaultSchema();
                        alias = str.Substring(str.IndexOf(" ") + 1);
                        tableAliases.Add(alias, authTab);
                    }
                    else {
                        //could be "table"
                        authTab.TableName = str;
                        authTab.SchemaName = this.GetDefaultSchema();
                        alias = String.Format("{0}.{1}", authTab.SchemaName, authTab.TableName).GetHashCode().ToString();
                        tableAliases.Add(alias, authTab);

                        //add an aditional alias cover all bases
                        alias = String.Format("{0}.{1}", authTab.SchemaName, authTab.TableName).GetHashCode().ToString();

                        if (!tableAliases.ContainsKey(alias))
                            tableAliases.Add(alias, authTab);
                    }
                }
            }//end loop

            return tableAliases;
        }//end method

        /// <summary>
        /// Determine the bulk load automatically authorized tables. 
        /// </summary>
        /// <returns></returns>
        private AuthorizedTable[] DefaultAuthorizedTables() {
            return new AuthorizedTable[] {
                new AuthorizedTable { TableName = "BULK_ITEMS_CHAR", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_DATETIME", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BUK_ITEMS_DECIMAL", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_FLOAT", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_INT16", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_INT32", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_INT64", SchemaName = "CBLOCK" },
                new AuthorizedTable { TableName = "BULK_ITEMS_STRING", SchemaName = "CBLOCK" }
            };
        }//end method

        /// <summary>
        /// Gets the connection string used to connect to the database. 
        /// </summary>
        /// <returns></returns>
        private String GetConnectionString() {
            String connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
            return connStr;
        }

        /// <summary>
        /// Get the default schema for this user. 
        /// </summary>
        /// <returns></returns>
        public String GetDefaultSchema() {
            OracleConnection conn = null;
            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    String generatedSql = "CBLOCK.ADHOCKEY.GET_DEFAULT_SCHEMA";

                    cmd.CommandText = generatedSql;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("returnVal", OracleDbType.Varchar2, 64);
                    cmd.Parameters["returnVal"].Direction = ParameterDirection.ReturnValue;

                    cmd.ExecuteNonQuery();

                    return cmd.Parameters["returnVal"].Value.ToString();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
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