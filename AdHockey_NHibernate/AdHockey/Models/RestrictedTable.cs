/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace AdHockey.Models {
    /// <summary>
    /// Fields to restrict based on group. A black list. 
    /// </summary>
    public class RestrictedTable {
        /// <summary>
        /// Unique identifier for restricted fields
        /// </summary>
        public virtual int FieldId {
            get; set;
        }

        /// <summary>
        /// Foreign key 
        /// </summary>
        public virtual int GroupId {
            get; set;
        }

        /// <summary>
        /// The group this restriction is associated with. 
        /// </summary>
        public virtual AdHockey.Models.Group Group {
            get; set;
        }

        /// <summary>
        /// Database schema name. 
        /// </summary>
        public virtual String SchemaName {
            get; set;
        }

        /// <summary>
        /// Database table name. 
        /// </summary>
        public virtual String TableName {
            get; set;
        }

        /// <summary>
        /// Full Description of the field in question. 
        /// </summary>
        public virtual String Description {
            get; set;
        }

        /// <summary>
        /// Determine whether sql is authorized to run. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool IsAuthorized(String sql) {
            //Debug.WriteLine("Contains A: " + sql.Contains(String.Format("{0}.{1}", SchemaName, TableName)));
            //Debug.WriteLine("Contains B: " + sql.Contains(String.Format("{0}", TableName)));
            //Debug.WriteLine("retVal: " + !(sql.Contains(String.Format("{0}.{1}", SchemaName, TableName))
            //    || sql.Contains(String.Format("{0}", TableName))));
            return !(sql.Contains(String.Format("{0}.{1}", SchemaName, TableName))
                || sql.Contains(String.Format("{0}", TableName)));
        }//end method
    }//end class

}//end namespace