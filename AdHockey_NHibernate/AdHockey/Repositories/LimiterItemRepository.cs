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

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Repositories {
    public class LimiterItemRepository : HibernateRepository<LimiterItem, int> {
        public LimiterItemRepository(){
            this.session = OpenSession();
        }

        /// <summary>
        /// Removes a LimiterItem from the repository based on it's unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        public override void Delete(int UserId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                LimiterItem entity = (LimiterItem)session.Load(typeof(LimiterItem), UserId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a LimiterItem from the database based on it's unique database identifier. 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public override LimiterItem GetById(int UserId) {
            ISession session = OpenSession();
            return (LimiterItem)session.Load(typeof(LimiterItem), UserId);
        }

        /// <summary>
        /// Inserts a LimiterItem into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(LimiterItem entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Updates a LimiterItem in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(LimiterItem entity) {
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
        public List<LimiterItem> GetUsersPaged(int pageNumber, int pageSize, int reportId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var Users = session.Query<LimiterItem>()
                    .Where(User => User.ReportId == reportId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (Users == null)
                    return new List<LimiterItem>();

                return Users;
            }
        }//end method

        /// <summary>
        /// Search for Users in the database and retrieve a page of them. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<LimiterItem> SearchUsersPaged(String searchStr, int pageNumber, int pageSize) {
            List<LimiterItem> Users = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(LimiterItem));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("UserName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                Users = criteria.List<LimiterItem>().ToList();
            }

            if (Users == null)
                return new List<LimiterItem>();

            return Users;
        }//end method

        /// <summary>
        /// Retrieve a list of User names from the database for use in search box Auto-Complete. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetLimiterNames() {
            List<String> limiterNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                limiterNames = session.Query<LimiterItem>()
                    .Select(li => li.TemplateName)
                    .ToList();
            }

            return limiterNames;
        }//end method

        /// <summary>
        /// Retrieves the total number of search Users for a given search query. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the total number of Users in the database. 
        /// </summary>
        /// <returns></returns>
        public int GetNumUsers() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<LimiterItem>().Count();
            }
        }//end method

    }//end class
}//end namespace