using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {


    /// <summary>
    /// Used by report object to toggle on and off 'where' clauses based on GUI input. 
    /// </summary>
    public class LimiterItem : Template {

        /// <summary>
        /// Boolean value that determines whether limiter, a sql clause is turned on or off. 
        /// </summary>
        public virtual bool BoolVal {
            get; set;
        }

        public LimiterItem() { }
    }//end class
}//end namespace