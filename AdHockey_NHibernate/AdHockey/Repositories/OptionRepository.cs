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
using AdHockey.Models;

namespace AdHockey.Repositories {

    public class OptionRepository : HibernateRepository<Option, int>, IDisposable {
        public OptionRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Deletes a group from the database. 
        /// </summary>
        /// <param name="optionId"></param>
        public override void Delete(int optionId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Option entity = (Option)session.Load(typeof(Option), optionId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a group from the database by id. 
        /// </summary>
        /// <param name="optionId"></param>
        /// <returns></returns>
        public override Option GetById(int optionId) {
            ISession session = OpenSession();
            return (Option)session.Load(typeof(Option), optionId);
        }

        /// <summary>
        /// Inserts a group into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(Option entity) {
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
        public override void Update(Option entity) {
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
        public List<Option> GetOptionsPaged(int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var Options = session.Query<Option>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (Options == null)
                    return new List<Option>();

                return Options;
            }
        }//end method

        /// <summary>
        /// Retrieves a page of groups from the database in a searched fashion. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Option> SearchOptionsPaged(String searchStr, int pageNumber, int pageSize) {
            List<Option> groups = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Option));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("OptionValue", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                groups = criteria.List<Option>().ToList();
            }

            if (groups == null)
                return new List<Option>();

            return groups;
        }//end method

        /// <summary>
        /// Retrieves group names from the database for use in a search box. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetOptionValues() {
            List<String> groupNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                groupNames = session.Query<Option>()
                    .Select(opt => opt.OptionValue)
                    .ToList();
            }

            return groupNames;
        }//end method

        /// <summary>
        /// Gets the total number of groups. 
        /// </summary>
        /// <returns></returns>
        public int GetNumOptions() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Option>().Count();
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
                return session.Query<Option>()
                    .Where(opt => keyWords.Any(str => opt.OptionValue.Contains(str)))
                    .Count();
            }
        }//end method

    }//end class

}//end namespace