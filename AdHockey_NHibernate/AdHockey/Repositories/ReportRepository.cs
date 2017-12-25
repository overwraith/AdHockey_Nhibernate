/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Context;
using NHibernate.Transform;

using AdHockey.Repositories;
using AdHockey.Models;

namespace AdHockey.Repositories {

    public class ReportRepository : HibernateRepository<Report, int>, IDisposable {
        public ReportRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a report from the database via unique identifier. 
        /// </summary>
        /// <param name="reportId"></param>
        public override void Delete(int reportId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Report entity = (Report)session.Load(typeof(Report), reportId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a report from the database via a unique database identifier. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public override Report GetById(int reportId) {
            ISession session = OpenSession();
            return (Report)session.Load(typeof(Report), reportId);
        }

        /// <summary>
        /// Insert a report into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(Report entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update an existing report in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(Report entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }


        public List<Report> GetUsersReportsPaged(User user, int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var sql = "SELECT \r\n"
                        + "    {RPT.*} \r\n"
                        + "FROM CBLOCK.REPORT {RPT} \r\n"
                        + "WHERE RPT.REPORT_ID IN ( \r\n"
                        + "    SELECT \r\n"
                        + "        RPT.REPORT_ID \r\n"
                        + "    FROM CBLOCK.REPORT RPT \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.RPT_USR_BRIDGE BR_1 \r\n"
                        + "        ON RPT.REPORT_ID = BR_1.REPORT_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"USER\" USR \r\n"
                        + "        ON BR_1.USER_ID = BR_1.USER_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.RPT_GRP_BRIDGE BR_2 \r\n"
                        + "        ON RPT.REPORT_ID = BR_2.REPORT_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"GROUP\" GRP \r\n"
                        + "        ON BR_2.\"GROUP_ID\" = GRP.\"GROUP_ID\" \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.GRP_USR_BRIDGE BR_3 \r\n"
                        + "        ON GRP.\"GROUP_ID\" = BR_3.\"GROUP_ID\" \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"USER\" USR_2 \r\n"
                        + "        ON USR_2.USER_ID = BR_3.USER_ID \r\n"
                        + "    WHERE USR.USER_ID = :USR_ID \r\n"
                        + "        OR USR_2.USER_ID = :USR_ID) \r\n";

                var reports = session.CreateSQLQuery(sql)
                    .AddEntity("RPT", typeof(Report))
                    .SetInt32("USR_ID", user.UserId)
                    .SetFirstResult((pageNumber - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .List<Report>()
                    .ToList();

                if (reports == null)
                    return new List<Report>();

                return reports;
            }
        }//end method

        public List<Report> SearchUsersReportsPaged(User user, String searchStr, int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var sql = "SELECT \r\n"
                        + "    {RPT.*} \r\n"
                        + "FROM CBLOCK.REPORT {RPT} \r\n"
                        + "WHERE RPT.REPORT_ID IN ( \r\n"
                        + "    SELECT \r\n"
                        + "        RPT.REPORT_ID \r\n"
                        + "    FROM CBLOCK.REPORT RPT \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.RPT_USR_BRIDGE BR_1 \r\n"
                        + "        ON RPT.REPORT_ID = BR_1.REPORT_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"USER\" USR \r\n"
                        + "        ON BR_1.USER_ID = BR_1.USER_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.RPT_GRP_BRIDGE BR_2 \r\n"
                        + "        ON RPT.REPORT_ID = BR_2.REPORT_ID \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"GROUP\" GRP \r\n"
                        + "        ON BR_2.\"GROUP_ID\" = GRP.\"GROUP_ID\" \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.GRP_USR_BRIDGE BR_3 \r\n"
                        + "        ON GRP.\"GROUP_ID\" = BR_3.\"GROUP_ID\" \r\n"
                        + "    LEFT OUTER JOIN CBLOCK.\"USER\" USR_2 \r\n"
                        + "        ON USR_2.USER_ID = BR_3.USER_ID \r\n"
                        + "    WHERE USR.USER_ID = :USR_ID \r\n"
                        + "        OR USR_2.USER_ID = :USR_ID) \r\n";

                String[] toks = searchStr.ToLower().Split();
                for(int i = 0; i < toks.Length; i++)
                    sql += "    OR "+ String.Format("RPT.REPORT_NAME LIKE '%{0}%' \r\n", toks[i]);

                var reports = session.CreateSQLQuery(sql)
                    .AddEntity("RPT", typeof(Report))
                    .SetInt32("USR_ID", user.UserId)
                    .SetFirstResult((pageNumber - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .List<Report>()
                    .ToList();

                if (reports == null)
                    return new List<Report>();

                return reports;
            }
        }//end method

        //public List<Report> SearchUsersReportsPaged(User user, String searchStr, int pageNumber, int pageSize) {
        //    List<Report> reports = null;
        //    String[] toks = searchStr.ToLower().Split();
        //    ISession session = OpenSession();
        //    using (ITransaction tx = session.BeginTransaction()) {
        //        ICriteria criteria1 = session.CreateCriteria(typeof(Report));
        //        criteria1.CreateCriteria("Users")
        //            .Add(Restrictions.Eq("UserId", user.UserId));
        //        criteria1.SetProjection(Projections.Distinct(Projections.Property("ReportId")));

        //        ICriteria criteria2 = session.CreateCriteria(typeof(Report));
        //        criteria2.CreateCriteria("Groups")
        //            .CreateCriteria("Users")
        //                .Add(Restrictions.Eq("UserId", user.UserId));
        //        criteria2.SetProjection(Projections.Distinct(Projections.Property("ReportId")));
        //        //case insensitive search
        //        foreach (var tok in toks) {
        //            criteria1.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());
        //            criteria2.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());
        //        }

        //        List<Report> temp = criteria1.List<Report>().ToList();
        //        temp.AddRange(criteria1.List<Report>().ToList());
        //        reports = temp
        //            .Skip(pageSize * pageNumber)
        //            .Take(pageSize)
        //            .Distinct()
        //            .ToList();
        //    }

        //    if (reports == null)
        //        return new List<Report>();

        //    return reports;
        //}//end method

        public int GetNumUsersReports(User user) {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Report>()
                    .Where(rpt => rpt.Users.Contains(user))
                    .Where(rpt => rpt.Groups.Where(grp => grp.Users.Contains(user)).Count() > 1)
                    .Count();
            }
        }//end method

        public int GetTotalNumUsersSearchResults(User user, String searchStr) {
            int count = -1;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Report));
                criteria.CreateCriteria("Users")
                    .Add(Restrictions.Eq("UserId", user.UserId))
                    .CreateCriteria("Groups")
                    .CreateCriteria("Users")
                        .Add(Restrictions.Eq("UserId", user.UserId));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());

                count = criteria.List<Report>().Count();
            }

            return count;
        }//end method

        /// <summary>
        /// Retrieve a page of reports from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> GetReportsPaged(int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var reports = session.Query<Report>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (reports == null)
                    return new List<Report>();

                return reports;
            }
        }//end method

        /// <summary>
        /// Search for reports in the database and retrieve a page of them. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> SearchReportsPaged(String searchStr, int pageNumber, int pageSize) {
            List<Report> reports = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Report));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                reports = criteria.List<Report>().ToList();
            }

            if (reports == null)
                return new List<Report>();

            return reports;
        }//end method

        /// <summary>
        /// Gets the count of the number of reports in the database. 
        /// </summary>
        /// <returns></returns>
        public int GetNumReports() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Report>().Count();
            }
        }//end method

        /// <summary>
        /// Gets the count of the number of reports in the database after a search has been applied. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public int GetTotalNumSearchResults(String searchStr) {
            int count = -1;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Report));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());

                count = criteria.List<Report>().Count();
            }

            return count;
        }//end method

        /// <summary>
        /// Retrieve report names from database for search box Auto-Complete
        /// </summary>
        /// <returns></returns>
        public List<String> GetReportNames() {
            List<String> reportNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                reportNames = session.Query<Report>()
                    .Select(report => report.ReportName )
                    .ToList();
            }

            return reportNames;
        }//end method

        /// <summary>
        /// Gets a page of reports for a given User. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> GetUserReportsPaged(int userId, int pageNumber, int pageSize) {
            List<Report> groups = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                groups = session.Query<User>()
                        .Where(usr => usr.UserId == userId)
                        .SelectMany(rpt => rpt.Reports)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
            }

            if (groups == null)
                return new List<Report>();

            return groups;
        }//end method

        /// <summary>
        /// Searches for a page of reports for a given user. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="userId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> SearchUserReportsPaged(String searchStr, int userId, int pageNumber, int pageSize) {
            List<Report> users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT rpt FROM\r\n"
                             + " User as usr\r\n"
                             + " JOIN usr.Reports as rpt\r\n"
                             + "WHERE usr.UserId = :userId\r\n"
                             + " AND lower(rpt.ReportName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("userId", userId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(rpt.ReportName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                users = query.SetFirstResult((pageNumber - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(Report)))
                    .List<Report>()
                    .ToList();
            }//end using

            if (users == null)
                return new List<Report>();

            return users;
        }//end method

        /// <summary>
        /// Gets report names for a specific user. 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<String> GetUserReportNames(int userId) {
            List<String> groupNames = new List<String>();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {

                String hql = "SELECT DISTINCT rpt.{0} \r\n"
                    + "FROM User as usr \r\n"
                    + "JOIN usr.Reports as rpt \r\n"
                    + "WHERE usr.UserId = :userId\r\n";

                var groupNames_A = session.CreateQuery(String.Format(hql, "ReportName"))
                    .SetInt32("userId", userId)
                    .List<String>()
                    .ToList();

                groupNames.AddRange(groupNames_A);
            }

            return groupNames;
        }//end method

        /// <summary>
        /// Retrieves the number of reports for a user given a specific search query. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetTotalNumUserReportSearchResults(String searchStr, int userId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT count(rpt) FROM\r\n"
                             + " User as usr\r\n"
                             + " JOIN usr.Reports as rpt\r\n"
                             + "WHERE usr.UserId = :userId\r\n"
                             + " AND lower(rpt.UserName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("userId", userId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(rpt.ReportName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                return (int)query.UniqueResult();
            }//end using

        }//end method

        /// <summary>
        /// Gets the total number of reports for a given user. 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetUserNumReports(int userId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String hql = "SELECT count(usr) FROM\r\n"
                             + " User as usr\r\n"
                             + " JOIN rpt.Reports as rpt\r\n"
                             + "WHERE usr.UserId = :userId\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("userId", userId);

                return (int)query.UniqueResult();
            }
        }//end method

    }//end class

}//end namespace