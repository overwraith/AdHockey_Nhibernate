/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Cfg;
using NHibernate;

namespace AdHockey.Repositories {

    public abstract class HibernateRepository<TEntity, TKey> : IRepository<TEntity, TKey>, IDisposable where TEntity : class {
        protected static ISessionFactory sessionFactory;
        protected static Configuration config = null;
        protected ISession session = null;
        protected ITransaction transaction = null;

        protected static ISessionFactory SessionFactory {
            get {
                if (sessionFactory == null) {
                    config = new Configuration();
                    config.Configure();
                    config.AddAssembly("AdHockey");
                    sessionFactory = config.BuildSessionFactory();
                }
                return sessionFactory;
            }
        }

        public ISession Session {
            get { return session; }
        }

        public static ISession OpenSession() {
            return SessionFactory.OpenSession();
        }

        public void BeginTransaction() {
            transaction = session.BeginTransaction();
        }

        public void CommitTransaction() {
            transaction.Commit();
            CloseTransaction();
        }

        public void RollbackTransaction() {
            transaction.Rollback();
            CloseTransaction();
            CloseSession();
        }

        private void CloseSession() {
            session.Close();
            session.Dispose();
            session = null;
        }

        private void CloseTransaction() {
            transaction.Dispose();
            transaction = null;
        }

        public abstract void Delete(TKey id);
        public abstract TEntity GetById(TKey id);
        public abstract void Insert(TEntity entity);
        public abstract void Update(TEntity entity);

        public void Dispose() {
            if (transaction != null) {
                CommitTransaction();
            }
            if(session != null){
                session.Flush();
                CloseSession();
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing && session != null)
                    session.Dispose();
            }
            disposed = true;
        }//end method

    }//end class

}//end namespace