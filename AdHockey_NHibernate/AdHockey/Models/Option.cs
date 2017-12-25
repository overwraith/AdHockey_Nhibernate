using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {

    /// <summary>
    /// Options are used in drop down lists. 
    /// </summary>
    public class Option {
        public Option() {
        }

        /// <summary>
        /// Primary constructor with non-translated values. 
        /// </summary>
        /// <param name="value"></param>
        public Option(String value) {
            OptionValue = value;
            OptionTransform = ' ';
        }

        /// <summary>
        /// Optional translation form of the option object. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="transVal"></param>
        public Option(String value, char transVal) {
            OptionValue = value;
            OptionTransform = transVal;
        }

        /// <summary>
        /// Unique identifier for option table. 
        /// </summary>
        public virtual int OptionId {
            get; set;
        }

        /// <summary>
        /// The Template the options are associated with. 
        /// </summary>
        public virtual int TemplateId {
            get; set;
        }

        /// <summary>
        /// The User associated with this option. 
        /// </summary>
        public virtual Template Template {
            get; set;
        }

        /// <summary>
        /// The value that appears in the drop down list. 
        /// </summary>
        public virtual String OptionValue {
            get; set;
        }

        /// <summary>
        /// The optional character value a string is transformed into. 
        /// </summary>
        public virtual char OptionTransform {
            get; set;
        }
    }//end class

}//end namespace