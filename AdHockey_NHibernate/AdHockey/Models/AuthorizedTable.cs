using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdHockey.Models;

namespace AdHockey.Models {
    /// <summary>
    /// Fields to restrict based on group. A white list. 
    /// </summary>
    public class AuthorizedTable {
        /// <summary>
        /// Unique identifier for authorized fields. 
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
        /// Overridden equals method. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;
            AuthorizedTable other = (AuthorizedTable)obj;
            
            return this.GetHashCode() == other.GetHashCode();
        }//end method

        /// <summary>
        /// Overridden get hash code method. 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return SchemaName.GetHashCode() ^ TableName.GetHashCode();
        }//end method

    }//end class

}//end namespace