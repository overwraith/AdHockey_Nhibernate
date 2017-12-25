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

    public class RestrictedTableRepository : HibernateRepository<RestrictedTable, int> {
        public RestrictedTableRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a RestrictedTable from the database using a unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                RestrictedTable entity = (RestrictedTable)session.Load(typeof(RestrictedTable), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a RestrictedTable from the database using unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override RestrictedTable GetById(int UserId) {
            ISession session = OpenSession();
            return (RestrictedTable)session.Load(typeof(RestrictedTable), UserId);
        }

        /// <summary>
        /// Insert a RestrictedTable into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(RestrictedTable entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a RestrictedTable in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(RestrictedTable entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a page of Users from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<RestrictedTable> GetUsersPaged(int pageNumber, int pageSize) {
            List<RestrictedTable> Users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<RestrictedTable>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Users == null)
                return new List<RestrictedTable>();

            return Users;
        }//end method

        /// <summary>
        /// Search for Users and retrieve a page of them from the database. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<RestrictedTable> SearchUsersPaged(String searchStr, int pageNumber, int pageSize) {
            List<RestrictedTable> authTabs = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(RestrictedTable));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("TableName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                authTabs = criteria.List<RestrictedTable>().ToList();
            }

            if (authTabs == null)
                return new List<RestrictedTable>();

            return authTabs;
        }//end method

        /// <summary>
        /// Get User Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetRestrictedTableNames() {
            List<String> UserNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                UserNames = session.Query<RestrictedTable>()
                    .Select(authTab => authTab.TableName)
                    .ToList();
            }

            return UserNames;
        }//end method

        public int GetTotalNumSearchResults(String searchStr) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<RestrictedTable>()
                    .Where(authTab => keyWords.Any(str => authTab.TableName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<RestrictedTable>().Count();
            }
        }//end method

    }//end class

}//end namespace