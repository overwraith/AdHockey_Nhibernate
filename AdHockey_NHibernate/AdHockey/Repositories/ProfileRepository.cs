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

    public class ProfileRepository : HibernateRepository<Profile, int>, IDisposable {
        public ProfileRepository() {
            this.session = OpenSession();
        }

        /// <summary>
        /// Deletes a group from the database. 
        /// </summary>
        /// <param name="groupId"></param>
        public override void Delete(int groupId) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                Profile entity = (Profile)session.Load(typeof(Profile), groupId);
                NHibernate.Context.CurrentSessionContext.Bind(session);
                session.Delete(entity);
                tx.Commit();
            }
        }

        /// <summary>
        /// Retrieves a group from the database by id. 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public override Profile GetById(int groupId) {
            ISession session = OpenSession();
            return (Profile)session.Load(typeof(Profile), groupId);
        }

        /// <summary>
        /// Inserts a group into the database. 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(Profile entity) {
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
        public override void Update(Profile entity) {
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
        public List<Profile> GetProfilesPaged(int pageNumber, int pageSize) {
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                var Profiles = session.Query<Profile>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (Profiles == null)
                    return new List<Profile>();

                return Profiles;
            }
        }//end method

        /// <summary>
        /// Retrieves a page of groups from the database in a searched fashion. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Profile> SearchProfilesPaged(String searchStr, int pageNumber, int pageSize) {
            List<Profile> groups = null;
            String[] toks = searchStr.ToLower().Split();
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                ICriteria criteria = session.CreateCriteria(typeof(Profile));
                //case insensitive search
                foreach (var tok in toks)
                    criteria.Add(Expression.Like("ProfileName", "%" + tok + "%").IgnoreCase());

                //pagination code
                criteria.SetFirstResult((pageNumber - 1) * pageSize);
                criteria.SetMaxResults(pageSize);

                groups = criteria.List<Profile>().ToList();
            }

            if (groups == null)
                return new List<Profile>();

            return groups;
        }//end method

        /// <summary>
        /// Retrieves group names from the database for use in a search box. 
        /// </summary>
        /// <returns></returns>
        public List<String> GetProfileNames() {
            List<String> groupNames = null;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                groupNames = session.Query<Profile>()
                    .Select(grp => grp.ProfileName)
                    .ToList();
            }

            return groupNames;
        }//end method

        /// <summary>
        /// Gets the total number of groups. 
        /// </summary>
        /// <returns></returns>
        public int GetNumProfiles() {
            int count = 0;
            ISession session = OpenSession();
            using (ITransaction tx = session.BeginTransaction()) {
                return count = session.Query<Profile>().Count();
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
                return session.Query<Profile>()
                    .Where(grp => keyWords.Any(str => grp.ProfileName.Contains(str)))
                    .Count();
            }
        }//end method

    }//end class

}//end namespace