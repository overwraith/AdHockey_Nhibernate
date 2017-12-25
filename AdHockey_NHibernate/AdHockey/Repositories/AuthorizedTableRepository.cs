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

    public class AuthorizedTableRepository : HibernateRepository<AuthorizedTable, int> {
        public AuthorizedTableRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a AuthorizedTable from the database using a unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                AuthorizedTable entity = (AuthorizedTable)session.Load(typeof(AuthorizedTable), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a AuthorizedTable from the database using unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override AuthorizedTable GetById(int UserId) {
            ISession session = OpenSession();
            return (AuthorizedTable)session.Load(typeof(AuthorizedTable), UserId);
        }

        /// <summary>
        /// Insert a AuthorizedTable into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(AuthorizedTable entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a AuthorizedTable in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AuthorizedTable entity) {
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
        public List<AuthorizedTable> GetTablesPaged(int pageNumber, int pageSize) {
            List<AuthorizedTable> Users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<AuthorizedTable>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Users == null)
                return new List<AuthorizedTable>();

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
        public List<AuthorizedTable> SearchTablesPaged(String searchStr, int pageNumber, int pageSize) {
            List<AuthorizedTable> authTabs = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(AuthorizedTable));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("TableName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                authTabs = criteria.List<AuthorizedTable>().ToList();
            }

            if (authTabs == null)
                return new List<AuthorizedTable>();

            return authTabs;
        }//end method

        /// <summary>
        /// Get User Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetAuthorizedTableNames() {
            List<String> UserNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                UserNames = session.Query<AuthorizedTable>()
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
                return session.Query<AuthorizedTable>()
                    .Where(authTab => keyWords.Any(str => authTab.TableName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<AuthorizedTable>().Count();
            }
        }//end method

    }//end class

}//end namespace