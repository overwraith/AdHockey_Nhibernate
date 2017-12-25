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

    public class TemplateItemRepository : HibernateRepository<TemplateItem, int> {
        public TemplateItemRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a TemplateItem from the database using a unique database identifier. 
        /// </summary>
        /// <param name="TempalteId"></param>
        public override void Delete(int TempalteId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                TemplateItem entity = (TemplateItem)session.Load(typeof(TemplateItem), TempalteId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a TemplateItem from the database using unique database identifier. 
        /// </summary>
        /// <param name="TempalteId"></param>
        /// <returns></returns>
        public override TemplateItem GetById(int TempalteId) {
            ISession session = OpenSession();
            return (TemplateItem)session.Load(typeof(TemplateItem), TempalteId);
        }

        /// <summary>
        /// Insert a TemplateItem into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(TemplateItem entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a TemplateItem in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(TemplateItem entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a page of Tempaltes from the database. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<TemplateItem> GetTemplatesPaged(int pageNumber, int pageSize) {
            List<TemplateItem> Tempaltes = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Tempaltes = session.Query<TemplateItem>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (Tempaltes == null)
                return new List<TemplateItem>();

            return Tempaltes;
        }//end method

        /// <summary>
        /// Search for Tempaltes and retrieve a page of them from the database. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<TemplateItem> SearchTemplatesPaged(String searchStr, int pageNumber, int pageSize) {
            List<TemplateItem> Tempaltes = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(TemplateItem));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ReportName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                Tempaltes = criteria.List<TemplateItem>().ToList();
            }

            if (Tempaltes == null)
                return new List<TemplateItem>();

            return Tempaltes;
        }//end method

        /// <summary>
        /// Get Tempalte Item names from database. Used for search box auto fill. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetTemplateItemNames() {
            List<String> TempalteNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                TempalteNames = session.Query<TemplateItem>()
                    .Select(template => template.TemplateName)
                    .ToList();
            }

            return TempalteNames;
        }//end method

        public int GetTotalNumSearchResults(String searchStr) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return session.Query<TemplateItem>()
                    .Where(template => keyWords.Any(str => template.TemplateName.Contains(str)))
                    .Count();
            }
        }//end method

        public int GetNumTemplates() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<TemplateItem>().Count();
            }
        }//end method

    }//end class

}//end namespace