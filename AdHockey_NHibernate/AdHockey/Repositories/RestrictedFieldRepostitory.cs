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

    public class RestrictedFieldRepository : HibernateRepository<RestrictedField, int> {
        public RestrictedFieldRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a RestrictedField from the database using a unique database identifier. 
        /// </summary>
        /// <param name="RestrictedFieldId"></param>
        public override void Delete(int RestrictedFieldId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                RestrictedField entity = (RestrictedField)session.Load(typeof(RestrictedField), RestrictedFieldId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a RestrictedField from the database using unique database identifier. 
        /// </summary>
        /// <param name="RestrictedFieldId"></param>
        /// <returns></returns>
        public override RestrictedField GetById(int RestrictedFieldId) {
            ISession session = OpenSession();
            return (RestrictedField)session.Load(typeof(RestrictedField), RestrictedFieldId);
        }

        /// <summary>
        /// Insert a RestrictedField into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(RestrictedField entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a RestrictedField in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(RestrictedField entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a page of RestrictedFields from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<RestrictedField> GetRestrictedFieldsPaged(int pageNumber, int pageSize) {
            List<RestrictedField> RestrictedFields = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                RestrictedFields = session.Query<RestrictedField>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (RestrictedFields == null)
                return new List<RestrictedField>();

            return RestrictedFields;
        }//end method

        /// <summary>
        /// Search for RestrictedFields and retrieve a page of them from the database. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<RestrictedField> SearchRestrictedFieldsPaged(String searchStr, int pageNumber, int pageSize) {
            List<RestrictedField> authTabs = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(RestrictedField));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ColumnName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                authTabs = criteria.List<RestrictedField>().ToList();
            }

            if (authTabs == null)
                return new List<RestrictedField>();

            return authTabs;
        }//end method

        /// <summary>
        /// Get RestrictedField Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetRestrictedFieldNames() {
            List<String> RestrictedFieldNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                RestrictedFieldNames = session.Query<RestrictedField>()
                    .Select(authTab => authTab.TableName)
                    .ToList();
            }

            return RestrictedFieldNames;
        }//end method

        public int GetTotalNumSearchResults(String searchStr) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<RestrictedField>()
                    .Where(authTab => keyWords.Any(str => authTab.TableName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumRestrictedFields() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<RestrictedField>().Count();
            }
        }//end method

    }//end class

}//end namespace