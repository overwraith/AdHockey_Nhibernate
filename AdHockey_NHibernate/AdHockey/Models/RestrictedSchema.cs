/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace AdHockey.Models {
    /// <summary>
    /// Schema to restrict based on group. A white list. 
    /// </summary>
    public class RestrictedSchema {
        /// <summary>
        /// Unique identifier for restricted fields. 
        /// </summary>
        public virtual int FieldId {
            get; set;
        }

        /// <summary>
        /// Foreign key for the group table. 
        /// </summary>
        public virtual int GroupId {
            get; set;
        }

        /// <summary>
        /// The group this restriction is associated with. 
        /// </summary>
        public virtual Group Group {
            get; set;
        }

        /// <summary>
        /// Database schema name. 
        /// </summary>
        public virtual String SchemaName {
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
            //Debug.WriteLine("Contains: " + sql.Contains(String.Format("{0}", SchemaName)));
            //Debug.WriteLine("ReturnValue: " + !sql.Contains(String.Format("{0}", SchemaName)));
            return !sql.Contains(String.Format("{0}", SchemaName));
        }//end method
    }//end class

}//end namespace