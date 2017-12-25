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

    public class AuthorizedFieldRepository : HibernateRepository<AuthorizedField, int> {
        public AuthorizedFieldRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a AuthorizedField from the database using a unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                AuthorizedField entity = (AuthorizedField)session.Load(typeof(AuthorizedField), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a AuthorizedField from the database using unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override AuthorizedField GetById(int UserId) {
            ISession session = OpenSession();
            return (AuthorizedField)session.Load(typeof(AuthorizedField), UserId);
        }

        /// <summary>
        /// Insert a AuthorizedField into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(AuthorizedField entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a AuthorizedField in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AuthorizedField entity) {
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
        public List<AuthorizedField> GetUsersPaged(int pageNumber, int pageSize) {
            List<AuthorizedField> Users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<AuthorizedField>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Users == null)
                return new List<AuthorizedField>();

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
        public List<AuthorizedField> SearchFieldsPaged(String searchStr, int pageNumber, int pageSize) {
            List<AuthorizedField> authTabs = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(AuthorizedField));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ColumnName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                authTabs = criteria.List<AuthorizedField>().ToList();
            }

            if (authTabs == null)
                return new List<AuthorizedField>();

            return authTabs;
        }//end method

        /// <summary>
        /// Get User Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetAuthorizedFieldNames() {
            List<String> UserNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                UserNames = session.Query<AuthorizedField>()
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
                return session.Query<AuthorizedField>()
                    .Where(authTab => keyWords.Any(str => authTab.TableName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<AuthorizedField>().Count();
            }
        }//end method

    }//end class

}//end namespace