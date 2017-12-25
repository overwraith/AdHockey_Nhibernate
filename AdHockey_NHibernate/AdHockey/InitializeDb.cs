using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AdHockey.Models;
using AdHockey.Repositories;
using System.Text.RegularExpressions;

namespace AdHockey {

    public class InitializeDb {
        
        public InitializeDb(){
            InitializeReport();
            InitializeGroups();
            InitializeUsers();

            InitializeBulkTemplate();
            InitializeLimiterItem();
            InitializeTemplateItem();

            //InitializeRestrictedField();
            //InitializeRestrictedTable();
            //InitializeRestrictedSchema();

            //InitializeAuthorizedField();
            InitializeAuthorizedTable();
            InitializeAuthorizedSchema();
        }//end method

        private void InitializeUsers() {
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();

                User user = new User();
                user.FirstName = "Cameron";
                user.LastName = "Block";
                user.SetPassword("Madcat90");
                user.EmailAddress = "cnblock@cox.net";
                user.IsActive = true;

                //add user to group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }

                if(user.Groups == null)
                    user.Groups = new List<AdHockey.Models.Group>();

                user.Groups.Add(group);

                //add report to group
                Report report = null;
                using (ReportRepository reportRepo = new ReportRepository()) {
                    reportRepo.BeginTransaction();
                    report = reportRepo.GetById(1);
                }

                if (user.Reports == null)
                    user.Reports = new List<Report>();

                user.Reports.Add(report);

                usrRepo.Insert(user);

                usrRepo.CommitTransaction();
            }
        }//end method

        private void InitializeGroups() {
            using (GroupRepository tmpRepo = new GroupRepository()) {
                tmpRepo.BeginTransaction();
                AdHockey.Models.Group group = new AdHockey.Models.Group();

                group.GroupName = "The Users";
                group.Description = "Always find the bugs. ";

                tmpRepo.Insert(group);
                tmpRepo.CommitTransaction();
            }
        }//end method

        private void InitializeBulkTemplate() {
            using (BulkTemplateRepository tmpRepo = new BulkTemplateRepository()) {
                tmpRepo.BeginTransaction();
                BulkTemplate template = new BulkTemplate();
                template.ClrType = "System.String";
                template.Order = 1;
                template.ValueDescriptor = "SOME_NUM";
                template.TemplateName = "BulkTemplate";

                using (ReportRepository rptRepo = new ReportRepository()) {
                    rptRepo.BeginTransaction();
                    template.Report = rptRepo.GetById(1);
                }

                tmpRepo.Insert(template);
                tmpRepo.CommitTransaction();
            }
        }//end method

        private void InitializeLimiterItem() {
            using (LimiterItemRepository tmpRepo = new LimiterItemRepository()) {
                tmpRepo.BeginTransaction();
                LimiterItem template = new LimiterItem();
                template.Order = 1;
                template.TemplateName = "Limiter";

                using (ReportRepository rptRepo = new ReportRepository()) {
                    rptRepo.BeginTransaction();
                    template.Report = rptRepo.GetById(1);
                }

                tmpRepo.Insert(template);
                tmpRepo.CommitTransaction();
            }
        }//end method

        private void InitializeTemplateItem() {
            using (TemplateItemRepository tmpRepo = new TemplateItemRepository()) {
                tmpRepo.BeginTransaction();
                TemplateItem template = new TemplateItem();
                template.TemplateName = "TemplateItem";
                template.Alias = ":item";
                template.Order = 1;
                template.ClrType = "System.String";
                template.ControlName = "TextBox";

                using (ReportRepository rptRepo = new ReportRepository()) {
                    rptRepo.BeginTransaction();
                    template.Report = rptRepo.GetById(1);
                }

                tmpRepo.Insert(template);
                tmpRepo.CommitTransaction();
            }
        }//end method

        private void InitializeReport() {
            using (ReportRepository repo = new ReportRepository()) {
                repo.BeginTransaction();
                Report report = new Report();
                report.ReportName = "First Report";

                report.Sql = "SELECT "
                            + "    REPORT_KEY, TOGGLE, CREATE_DATE, DATA_A, DATA_B, DATA_C \r\n"
                            + "FROM CBLOCK.REPORT_B RPT\r\n"
                            + "--BULKTEMPLATE BulkTemplate \"1 = 1\"\r\n"
                            + "--LIMITATION Limiter \"WHERE TOGGLE=1\"\r\n";

                ////work on regular expression ensure user only inputs aliased sql
                ////[\s\r\n]*[^\.\s\r\n,] *[,][\s\r\n] * +[^\.\s\r\n,] +[\.][^\.\s\r\n,] +[\s\r\n] *[^\.\s\r\n,] +[\s\r\n] + FROM
                //Regex reg = new Regex(@"^SELECT[\s\r\n]+([^\.\s\r\n,]+[\.][^\.\s\r\n,]+)+");
                //Match match = reg.Match(report.Sql);

                report.Sql = "SELECT "
                            + "    RPT.REPORT_KEY, RPT.TOGGLE, RPT.CREATE_DATE, RPT.DATA_A, RPT.DATA_B, RPT.DATA_C \r\n"
                            + "FROM CBLOCK.REPORT_B RPT\r\n"
                            + "--BULKTEMPLATE BulkTemplate \"1 = 1\"\r\n"
                            + "--LIMITATION Limiter \"WHERE TOGGLE=1\"\r\n";

                report.Description = "Very first report for our new ad-hoc reporting engine. ";

                ////add user to group
                //AdHockey.Models.Group group = null;
                //using (GroupRepository grpRepo = new GroupRepository()) {
                //    grpRepo.BeginTransaction();
                //    group = grpRepo.GetById(1);
                //}

                //if (report.Groups == null)
                //    report.Groups = new List<AdHockey.Models.Group>();

                //report.Groups.Add(group);

                ////add user to report
                //User user = null;
                //using (UserRepository usrRepo = new UserRepository()) {
                //    usrRepo.BeginTransaction();
                //    user = usrRepo.GetById(2);
                //}

                //if (report.Users == null)
                //    report.Users = new List<User>();

                //report.Users.Add(user);

                repo.Insert(report);
                repo.CommitTransaction();
            }
        }//end method

        public void InitializeRestrictedField() {
            using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                repo.BeginTransaction();
                RestrictedField restriction = new RestrictedField();
                restriction.ColumnName = "DATA_A";
                restriction.Description = "The DATA_A column contains super secret data that we don't want the script kiddies to get their hands on. ";
                restriction.SchemaName = "CBLOCK";
                restriction.TableName = "REPORT_B";

                //get the group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }
                restriction.Group = group;

                repo.Insert(restriction);
            }
        }//end method

        public void InitializeRestrictedTable() {
            using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                repo.BeginTransaction();
                RestrictedTable restriction = new RestrictedTable();
                restriction.Description = "The REPORT_B table contains super secret data that we don't want the script kiddies to get their hands on. ";
                restriction.SchemaName = "CBLOCK";
                restriction.TableName = "REPORT_B";

                //get the group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }
                restriction.Group = group;

                repo.Insert(restriction);
            }
        }//end method

        public void InitializeRestrictedSchema() {
            using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                repo.BeginTransaction();
                RestrictedSchema restriction = new RestrictedSchema();
                restriction.Description = "The CBLOCK schema contains super secret data that we don't want the script kiddies to get their hands on. ";
                restriction.SchemaName = "CBLOCK";

                //get the group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }
                restriction.Group = group;

                repo.Insert(restriction);
            }
        }//end method

        //public void InitializeAuthorizedField() {
        //    String[] tableColumns = new String[] { "REPORT_KEY", "TOGGLE", "CREATE_DATE", "DATA_A", "DATA_B", "DATA_C" };
        //    foreach (var col in tableColumns) {
        //        using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
        //            repo.BeginTransaction();
        //            AuthorizedField restriction = new AuthorizedField();
        //            restriction.ColumnName = col;
        //            restriction.Description = "Non secret data. ";
        //            restriction.SchemaName = "CBLOCK";
        //            restriction.TableName = "REPORT_B";

        //            //get the group
        //            AdHockey.Models.Group group = null;
        //            using (GroupRepository grpRepo = new GroupRepository()) {
        //                grpRepo.BeginTransaction();
        //                group = grpRepo.GetById(2);
        //            }
        //            restriction.Group = group;

        //            repo.Insert(restriction);
        //        }
        //    }//end loop

        //}//end method

        public void InitializeAuthorizedTable() {
            using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                repo.BeginTransaction();
                AuthorizedTable restriction = new AuthorizedTable();
                restriction.Description = "The REPORT_B table contains super secret data that we don't want the script kiddies to get their hands on. ";
                restriction.SchemaName = "CBLOCK";
                restriction.TableName = "REPORT_B";

                //get the group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }
                restriction.Group = group;

                repo.Insert(restriction);
            }
        }//end method

        public void InitializeAuthorizedSchema() {
            using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                repo.BeginTransaction();
                AuthorizedSchema restriction = new AuthorizedSchema();
                restriction.Description = "The CBLOCK schema contains super secret data that we don't want the script kiddies to get their hands on. ";
                restriction.SchemaName = "CBLOCK";

                //get the group
                AdHockey.Models.Group group = null;
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    group = grpRepo.GetById(2);
                }
                restriction.Group = group;

                repo.Insert(restriction);
            }
        }//end method

    }//end class

}//end namespace