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

    public class SynopsisRepository : HibernateRepository<Synopsis, int> {
        public SynopsisRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Delete a Synopsis from the database using a unique database identifier. 
        /// </summary>
        /// <param name="synopsisId"></param>
        public override void Delete(int synopsisId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Synopsis entity = (Synopsis)session.Load(typeof(Synopsis), synopsisId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieve a Synopsis from the database using unique database identifier. 
        /// </summary>
        /// <param name="synopsisId"></param>
        /// <returns></returns>
        public override Synopsis GetById(int synopsisId) {
            ISession session = OpenSession();
            return (Synopsis)session.Load(typeof(Synopsis), synopsisId);
        }

        /// <summary>
        /// Insert a Synopsis into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(Synopsis entity) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Merge(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Update a Synopsis in the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(Synopsis entity) {
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
        public List<Synopsis> GetSynopsisPaged(int reportId, int pageNumber, int pageSize) {
            List<Synopsis> synopsises = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                synopsises = session.Query<Synopsis>()
                    .Where(syn => syn.ReportId == reportId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            if (synopsises == null)
                return new List<Synopsis>();

            return synopsises;
        }//end method

        /// <summary>
        /// Search for Users and retrieve a page of them from the database. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Synopsis> SearchSynopsisPaged(String searchStr, int pageNumber, int pageSize) {
            List<Synopsis> Users = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Synopsis));
                //case insensitive search
                foreach (var tok in toks) {
                    criteria.Add(Expression.Like("RecommenderFirstName", "%" + tok + "%").IgnoreCase());
                    criteria.Add(Expression.Like("RecommenderLastName", "%" + tok + "%").IgnoreCase());
                }

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                Users = criteria.List<Synopsis>().ToList();
            }

            if (Users == null)
                return new List<Synopsis>();

            return Users;
        }//end method

        /// <summary>
        /// Total number of search results involving recommender first name and last name. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public int GetTotalNumSearchResults(String searchStr, int reportId) {
            if (searchStr == null)
                throw new ArgumentException("Search text cannot be null. ");

            String[] keyWords = searchStr.Split(new char[] { ' ' });

            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var list1 = session.Query<Synopsis>()
                    .Where(synopsis => keyWords.Any(str => synopsis.RecommenderFirstName.Contains(str) && synopsis.ReportId == reportId))
                    .ToList();
                var list2 = session.Query<Synopsis>()
                    .Where(synopsis => keyWords.Any(str => synopsis.RecommenderLastName.Contains(str)) && synopsis.ReportId == reportId)
                    .ToList();
                list1.AddRange(list2);
                return list1.Count();
            }
        }//end method

        /// <summary>
        /// Total number of Synopsyses. 
        /// </summary>
        /// <returns></returns>
        public int GetNumSynopses() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Synopsis>().Count();
            }
        }//end method

        /// <summary>
        /// Retrieve a list of User names from the database for use in search box Auto-Complete. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetSynopsisNames() {
            List<String> synNames = new List<String>();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                session.Query<Synopsis>()
                    .Select(syn => syn.RecommenderFirstName)
                    .ToList<String>()
                    .Concat(synNames);

                session.Query<Synopsis>()
                    .Select(syn => syn.RecommenderLastName)
                    .ToList<String>()
                    .Concat(synNames);

                session.Query<Synopsis>()
                    .Select(syn => syn.ReccomenderTelephoneNum)
                    .ToList<String>()
                    .Concat(synNames);

                session.Query<Synopsis>()
                    .Select(syn => syn.ImplementerFirstName)
                    .ToList<String>()
                    .Concat(synNames);

                session.Query<Synopsis>()
                    .Select(syn => syn.ImplementerLastName)
                    .ToList<String>()
                    .Concat(synNames);

                session.Query<Synopsis>()
                    .Select(syn => syn.ImplementerTelephoneNum)
                    .ToList<String>()
                    .Concat(synNames);
            }
            synNames = synNames.Distinct().ToList();
            synNames.Sort();
            return synNames;
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
                return session.Query<Synopsis>()
                    .Where(syn => keyWords.Any(str => syn.ImplementerFirstName.Contains(str))
                    || keyWords.Any(str => syn.ImplementerLastName.Contains(str))
                    || keyWords.Any(str => syn.RecommenderFirstName.Contains(str))
                    || keyWords.Any(str => syn.RecommenderLastName.Contains(str))
                    || keyWords.Any(str => syn.ImplementerTelephoneNum.Contains(str))
                    || keyWords.Any(str => syn.ReccomenderTelephoneNum.Contains(str)))
                    .Count();
            }
        }//end method

        /// <summary>
        /// Gets the total number of Users in the database. 
        /// </summary>
        /// <returns></returns>
        public int GetNumSynopsises() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Synopsis>().Count();
            }
        }//end method

    }//end class

}//end namespace