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
using System.Globalization;

namespace AdHockey.Repositories {

    public class BulkTemplateRepository : HibernateRepository<BulkTemplate, int> {
        public BulkTemplateRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Deletes a specific User based on User id. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                BulkTemplate entity = (BulkTemplate)session.Load(typeof(BulkTemplate), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a specific User based on User id. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override BulkTemplate GetById(int UserId) {
            ISession session = OpenSession();
            return (BulkTemplate)session.Load(typeof(BulkTemplate), UserId);
        }

        /// <summary>
        /// Inserts a User into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(BulkTemplate entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Updates a User, saves changes into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(BulkTemplate entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Gets User names for usage in search box. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetTemplateNames() {
            ISession session = OpenSession();
            List<String> Users = null;
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<BulkTemplate>()
                    .Select(template => template.TemplateName)
                    .ToList();
            }
            return Users;
        }

        /// <summary>
        /// Gets a page of Users in an unsearched fashion. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<BulkTemplate> GetUsersPaged(int pageNumber, int pageSize) {
            List<BulkTemplate> Users = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Users = session.Query<BulkTemplate>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Users == null)
                return new List<BulkTemplate>();

            return Users;
        }//end method

        /// <summary>
        /// Searches for a page of Users. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<BulkTemplate> SearchUsersPaged(String searchStr, int pageNumber, int pageSize, int reportId) {
            List<BulkTemplate> Users = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(BulkTemplate));
                criteria.Add(Expression.Eq("ReportId", reportId).IgnoreCase());
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("UserName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                Users = criteria.List<BulkTemplate>().ToList();
            }

            if (Users == null)
                return new List<BulkTemplate>();

            return Users;
        }//end method

        /// <summary>
        /// Gets the total number of Users from the database. 
        /// </summary>
        /// <returns></returns>
        public int GetNumUsers() {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<BulkTemplate>().Count();
            }
        }//end method

        /// <summary>
        /// Gets the total number of Users in a search query. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public int GetTotalNumSearchResults(String searchStr, int reportId) {
            String[] toks = searchStr.ToLower().Split();

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<BulkTemplate>()
                    .Where(tmp => toks.Any(tok => tok.All(tok_A => tmp.TemplateName.Contains(tok_A))))
                    .Count();
            }
        }//end method

    }//end class

}//end namespace