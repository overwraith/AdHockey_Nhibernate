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

    public class GroupRepository : HibernateRepository<Group, int>, IDisposable {
        public GroupRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Deletes a group from the database. 
        /// </summary>
        /// <param name="groupId"></param>
        public override void Delete(int groupId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Group entity = (Group)session.Load(typeof(Group), groupId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a group from the database by id. 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public override Group GetById(int groupId) {
            ISession session = OpenSession();
            return (Group)session.Load(typeof(Group), groupId);
        }

        /// <summary>
        /// Inserts a group into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(Group entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Updates a group in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(Group entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a page of groups from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Group> GetGroupsPaged(int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var Groups = session.Query<Group>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (Groups == null)
                    return new List<Group>();

                return Groups;
            }
        }//end method

        /// <summary>
        /// Retrieves a page of groups from the database in a searched fashion. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Group> SearchGroupsPaged(String searchStr, int pageNumber, int pageSize) {
            List<Group> groups = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Group));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("GroupName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                groups = criteria.List<Group>().ToList();
            }

            if (groups == null)
                return new List<Group>();

            return groups;
        }//end method

        /// <summary>
        /// Retrieves group names from the database for use in a search box. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetGroupNames() {
            List<String> groupNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                groupNames = session.Query<Group>()
                    .Select(grp => grp.GroupName)
                    .ToList();
            }

            return groupNames;
        }//end method

        /// <summary>
        /// Gets the total number of groups. 
        /// </summary>
        /// <returns></returns>
        public int GetNumGroups() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Group>().Count();
            }
        }//end method

        /// <summary>
        /// Gets the total number of search results for a given search query. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public int GetTotalNumSearchResults(String searchStr) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<Group>()
                    .Where(grp => keyWords.Any(str => grp.GroupName.Contains(str)))
                    .Count();
            }
        }//end method

        /// <summary>
        /// Gets a page of groups from the database for a specific report. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Group> GetReportGroupsPaged(int reportId, int pageNumber, int pageSize) {
            List<Group> groups = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                groups = session.Query<Report>()
                        .Where(rpt => rpt.ReportId == reportId)
                        .SelectMany(rpt => rpt.Groups)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
            }

            if (groups == null)
                return new List<Group>();

            return groups;
        }//end method

        /// <summary>
        /// Searches for a page of groups for a given report. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="reportId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Group> SearchReportGroupsPaged(String searchStr, int reportId, int pageNumber, int pageSize) {
            List<Group> users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT grp FROM\r\n"
                             + " Report as rpt\r\n"
                             + " JOIN rpt.Groups as grp\r\n"
                             + "WHERE rpt.ReportId = :reportId\r\n"
                             + " AND lower(grp.GroupName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("reportId", reportId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(grp.GroupName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                users = query.SetFirstResult((pageNumber - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(Group)))
                    .List<Group>()
                    .ToList();
            }//end using

            if (users == null)
                return new List<Group>();

            return users;
        }//end method

        /// <summary>
        /// Retrieves group names for a given report for use in a search box. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<String> GetReportGroupNames(int reportId) {
            List<String> groupNames = new List<String>();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {

                String hql = "SELECT DISTINCT grp.{0} \r\n"
                    + "FROM Report as rpt \r\n"
                    + "JOIN rpt.Groups as grp \r\n"
                    + "WHERE rpt.ReportId = :reportId\r\n";

                var groupNames_A = session.CreateQuery(String.Format(hql, "GroupName"))
                    .SetInt32("reportId", reportId)
                    .List<String>()
                    .ToList();

                groupNames.AddRange(groupNames_A);
            }

            return groupNames;
        }//end method

        /// <summary>
        /// Retrieves tht total number of groups for a search query for a specific group. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public int GetTotalNumReportGroupSearchResults(String searchStr, int reportId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT count(grp) FROM\r\n"
                             + " Report as rpt\r\n"
                             + " JOIN rpt.Groups as grp\r\n"
                             + "WHERE rpt.ReportId = :reportId\r\n"
                             + " AND lower(rpt.ReportName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("reportId", reportId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(usr.GroupName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                return (int)query.UniqueResult();
            }//end using

        }//end method

        /// <summary>
        /// Gets the total number of groups for a specific report. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public int GetReportNumGroups(int reportId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String hql = "SELECT count(grp) FROM\r\n"
                             + " Report as grp\r\n"
                             + " JOIN rpt.Groups as grp\r\n"
                             + "WHERE rpt.ReportId = :reportId\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("reportId", reportId);

                return (int)query.UniqueResult();
            }
        }//end method

        /// <summary>
        /// Get all groups for a specific user. 
        /// </summary>
        /// <returns></returns>
        public List<Group> GetAllGroups(int userId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {

                var sql = "SELECT \r\n"
                            + "    {GRP.*} \r\n"
                            + "FROM CBLOCK.\"GROUP\" {GRP} \r\n"
                            + "JOIN CBLOCK.GRP_USR_BRIDGE BR_1 \r\n"
                            + "    ON BR_1.GROUP_ID = GRP.GROUP_ID \r\n"
                            + "JOIN CBLOCK.\"USER\" USR \r\n"
                            + "    ON USR.USER_ID = BR_1.USER_ID \r\n"
                            + "WHERE USR.USER_ID = :USR_ID \r\n";

                var groups = session.CreateSQLQuery(sql)
                    .AddEntity("GRP", typeof(Group))
                    .SetInt32("USR_ID", userId)
                    .List<Group>()
                    .ToList();

                if (groups == null)
                    return new List<Group>();

                return groups;
            }
        }//end method

    }//end class

}//end namespace