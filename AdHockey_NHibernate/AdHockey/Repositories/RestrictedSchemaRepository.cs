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

    public class RestrictedSchemaRepository : HibernateRepository<RestrictedSchema, int> {
        public RestrictedSchemaRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a RestrictedSchema from the database using a unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                RestrictedSchema entity = (RestrictedSchema)session.Load(typeof(RestrictedSchema), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a RestrictedSchema from the database using unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override RestrictedSchema GetById(int UserId) {
            ISession session = OpenSession();
            return (RestrictedSchema)session.Load(typeof(RestrictedSchema), UserId);
        }

        /// <summary>
        /// Insert a RestrictedSchema into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(RestrictedSchema entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a RestrictedSchema in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(RestrictedSchema entity) {
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
        public List<RestrictedSchema> GetUsersPaged(int pageNumber, int pageSize) {
            List<RestrictedSchema> Users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<RestrictedSchema>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Users == null)
                return new List<RestrictedSchema>();

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
        public List<RestrictedSchema> SearchUsersPaged(String searchStr, int pageNumber, int pageSize) {
            List<RestrictedSchema> authTabs = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(RestrictedSchema));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("SchemaName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                authTabs = criteria.List<RestrictedSchema>().ToList();
            }

            if (authTabs == null)
                return new List<RestrictedSchema>();

            return authTabs;
        }//end method

        /// <summary>
        /// Get User Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetRestrictedSchemaNames() {
            List<String> UserNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                UserNames = session.Query<RestrictedSchema>()
                    .Select(restrSchema => restrSchema.SchemaName)
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
                return session.Query<RestrictedSchema>()
                    .Where(restrSchema => keyWords.Any(str => restrSchema.SchemaName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<RestrictedSchema>().Count();
            }
        }//end method

    }//end class

}//end namespace