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

    public class UserRepository : HibernateRepository<User, int> {
        public UserRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a user. 
        /// </summary>
        /// <param name="userId"></param>
        public override void Delete(int userId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                User entity = (User)session.Load(typeof(User), userId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Get a user by it's unique database identifier. 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public override User GetById(int userId) {
            ISession session = OpenSession();
            return (User)session.Load(typeof(User), userId);
        }

        /// <summary>
        /// Insert a user into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(User entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a user in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(User entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Get a page of users from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<User> GetUsersPaged(int pageNumber, int pageSize) {
            List<User> users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                users = session.Query<User>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (users == null)
                return new List<User>();

            return users;
        }//end method

        /// <summary>
        /// Search for users, retrieve a page from the database. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<User> SearchUsersPaged(String searchStr, int pageNumber, int pageSize) {
            List<User> users = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(User));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("UserName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                users = criteria.List<User>().ToList();
            }

            if (users == null)
                return new List<User>();

            return users;
        }//end method

        /// <summary>
        /// Get user names from database. Used for Search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetUserNames() {
            List<String> userNames = new List<String>();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var userNames_A = session.Query<User>()
                    .Select(usr => usr.FirstName)
                    .Distinct()
                    .ToList();
                var userNames_B = session.Query<User>()
                    .Select(usr => usr.LastName)
                    .Distinct()
                    .ToList();

                userNames.AddRange(userNames_A);
                userNames.AddRange(userNames_B);
            }

            return userNames;
        }//end method

        public int GetTotalNumSearchResults(String searchStr) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<User>()
                    .Where(usr => keyWords.Any(str => usr.FirstName.Contains(str) || usr.LastName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<User>().Count();
            }
        }//end method

        //TODO: PLEASE TELL ME WHY THIS DOES NOT WORK. 
        //public IList<User> GetGroupUsersPaged(int groupId, int pageNumber, int pageSize) {
        //    List<User> users = null;
        //    ISession session = OpenSession();
        //    using (ITransaction tx = session.BeginTransaction()) {
        //        String query = "SELECT usr\r\n"
        //                     + "FROM Group as grp\r\n"
        //                     + " JOIN grp.Users as usr\r\n"
        //                     + "WHERE grp.GroupId = :groupId\r\n";
        //        users = (List<User>)session.CreateQuery(query)
        //                .SetInt32("groupId", groupId)
        //                .SetFirstResult((pageNumber - 1) * pageSize)
        //                .SetMaxResults(pageSize)
        //                .SetResultTransformer(Transformers.AliasToBean(typeof(User)))
        //                .List<User>();
        //    }

        //    if (users == null)
        //        return new List<User>();

        //    return users;
        //}//end method

        public List<User> GetGroupUsersPaged(int groupId, int pageNumber, int pageSize) {
            List<User> users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                users = session.Query<Group>()
                        .Where(grp => grp.GroupId == groupId)
                        .SelectMany(grp => grp.Users)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
            }

            if (users == null)
                return new List<User>();

            return users;
        }//end method

        public List<User> SearchGroupUsersPaged(String searchStr, int groupId, int pageNumber, int pageSize) {
            List<User> users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT usr FROM\r\n"
                             + " Group as grp\r\n"
                             + " JOIN grp.Users as usr\r\n"
                             + "WHERE grp.GroupId = :groupId\r\n"
                             + " AND lower(usr.UserName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("groupId", groupId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(usr.UserName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                users = query.SetFirstResult((pageNumber - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(User)))
                    .List<User>()
                    .ToList();
            }//end using

            if (users == null)
                return new List<User>();

            return users;
        }//end method

        public List<String> GetGroupUserNames(int groupId) {
            List<String> userNames = new List<String>();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {

                String hql = "SELECT DISTINCT usr.{0} \r\n"
                    + "FROM Group as grp \r\n"
                    + "JOIN grp.Users as usr \r\n"
                    + "WHERE grp.GroupId = :groupId\r\n";

                var userNames_A = session.CreateQuery(String.Format(hql, "FirstName"))
                    .SetInt32("groupId", groupId)
                    .List<String>()
                    .ToList();
                var userNames_B = session.CreateQuery(String.Format(hql, "LastName"))
                    .SetInt32("groupId", groupId)
                    .List<String>()
                    .ToList();

                userNames.AddRange(userNames_A);
                userNames.AddRange(userNames_B);
            }

            return userNames;
        }//end method

        public int GetTotalNumGroupUserSearchResults(String searchStr, int groupId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String searchStrB = searchStr.ToLower();
                String hql = "SELECT count(usr) FROM\r\n"
                             + " Group as grp\r\n"
                             + " JOIN grp.Users as usr\r\n"
                             + "WHERE grp.GroupId = :groupId\r\n"
                             + " AND lower(usr.UserName) IN(:params)\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("groupId", groupId);

                //sql has a flaw, can't user 'like' and 'in' in same context
                char ch = 'A';
                foreach (var str in searchStrB.Split()) {
                    hql += String.Format(" AND lower(usr.UserName) LIKE :param_{0}\r\n", ch);
                    query = query.SetString(String.Format("param_{0}"), String.Format("%{0}%", str));
                    ch++;
                }//end loop

                return (int)query.UniqueResult();
            }//end using

        }//end method

        public int GetGroupNumUsers(int groupId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String hql = "SELECT count(usr) FROM\r\n"
                             + " Group as grp\r\n"
                             + " JOIN grp.Users as usr\r\n"
                             + "WHERE grp.GroupId = :groupId\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetInt32("groupId", groupId);

                return (int)query.UniqueResult();
            }
        }//end method

        /// <summary>
        /// Used to determine if user login is valid. 
        /// </summary>
        public bool IsValidLogin(Login login) {
            bool isValid = false;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                User user = session.Query<User>()
                        .Where(usr => usr.EmailAddress.ToLower() == login.EmailAddress.ToLower() && usr.IsActive == true)
                        .ToList()
                        .First();

                isValid = user.ComparePassword(login.Password);
            }
            return isValid;
        }//end method

        /// <summary>
        /// Used by registration page to determine if user email address already exists in database. 
        /// </summary>
        /// <param name="emailAddr"></param>
        /// <returns></returns>
        public bool EmailExists(String emailAddr) {
            bool isValid = false;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                String hql = "SELECT count(usr) FROM\r\n"
                             + "FROM Users as usr\r\n"
                             + "WHERE usr.EmailAddress = :emailAddr\r\n";

                IQuery query = session.CreateQuery(hql)
                    .SetString("emailAddr", emailAddr);

                return (int)query.UniqueResult() > 0;
            }
            return isValid;
        }//end method

        /// <summary>
        /// Used to get an admin based on their email address. 
        /// </summary>
        public User GetByEmail(String emailAddress) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<User>()
                        .Where(usr => usr.EmailAddress.ToLower() == emailAddress.ToLower())
                        .First();
            }
        }//end method

    }//end class

}//end namespace