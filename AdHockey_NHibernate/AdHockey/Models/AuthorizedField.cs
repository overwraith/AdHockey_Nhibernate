/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//TODO: Temporarily removing from project until I have a working sql antlr parser. 

namespace AdHockey.Models {
    /// <summary>
    /// Fields to restrict based on group. A white list. 
    /// </summary>
    public class AuthorizedField {
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
        /// Database column name. 
        /// </summary>
        public virtual String ColumnName {
            get; set;
        }

        /// <summary>
        /// Full Description of the field in question. 
        /// </summary>
        public virtual String Description {
            get; set;
        }

        /// <summary>
        /// Determines whether sql is authorized to execute. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool IsAuthorized(String sql) {
            throw new NotImplementedException("Implement this method dummy. ");
            return false;
        }//end method

        /// <summary>
        /// Overridden equals method. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;
            AuthorizedField other = (AuthorizedField)obj;

            return this.GetHashCode() == other.GetHashCode();
        }//end method

        /// <summary>
        /// Overridden get hash code method. 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return SchemaName.GetHashCode() ^ TableName.GetHashCode() ^ ColumnName.GetHashCode();
        }//end method

    }//end class

}//end description