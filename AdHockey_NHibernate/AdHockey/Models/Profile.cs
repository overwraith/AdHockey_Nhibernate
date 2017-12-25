/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {

    /// <summary>
    /// Profiles used to store table configurations for tables. 
    /// </summary>
    public class Profile {
        /// <summary>
        /// Id for profiles for subsequent group default whitelists and black lists. 
        /// </summary>
        public int ProfileId {
            get; set;
        }

        /// <summary>
        /// The name of the given profile. 
        /// </summary>
        public virtual String ProfileName {
            get; set;
        }

        /// <summary>
        /// Authorized fields for this given profile. 
        /// </summary>
        public IList<AuthorizedField> AuthorizedFields {
            get; set;
        }

        /// <summary>
        /// Authorized tables for this given profile. 
        /// </summary>
        public IList<AuthorizedTable> AuthorizedTables {
            get; set;
        }

        /// <summary>
        /// Authorized schemas for this given profile. 
        /// </summary>
        public IList<AuthorizedSchema> AuthorizedSchemas {
            get; set;
        }

        /// <summary>
        /// Restricted fields associated with this schema. 
        /// </summary>
        public IList<RestrictedField> RestrictedFields {
            get; set;
        }

        /// <summary>
        /// Restricted tables associated with this given profile. 
        /// </summary>
        public IList<RestrictedTable> RestrictedTables {
            get; set;
        }

        /// <summary>
        /// Restricted Schemas associated with this given provile. 
        /// </summary>
        public IList<RestrictedSchema> RestrictedSchemas {
            get; set;
        }

    }//end class

}//end class