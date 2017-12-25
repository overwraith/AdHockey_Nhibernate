using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {
    /// <summary>
    /// Schema to restrict based on group. A white list. 
    /// </summary>
    public class AuthorizedSchema {
        /// <summary>
        /// Unique identifier for authorized fields. 
        /// </summary>
        public virtual int FieldId {
            get; set;
        }

        /// <summary>
        /// The group this restriction is associated with. 
        /// </summary>
        public virtual Group Group {
            get; set;
        }

        /// <summary>
        /// Foreign key 
        /// </summary>
        public virtual int GroupId {
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

    }//end class

}//end namespace