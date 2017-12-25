/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Repositories {

    public interface IRepository<TEntity, TKey> where TEntity : class {
        TEntity GetById(TKey id);
        void Update(TEntity entity);
        void Delete(TKey id);
        void Insert(TEntity entity);
    }//end interface

}//end namespace