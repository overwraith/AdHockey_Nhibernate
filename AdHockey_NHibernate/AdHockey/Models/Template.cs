using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdHockey.Models {

    /// <summary>
    /// Base class for applying parameters and sql drop in's to report sql. 
    /// </summary>
    public class Template {

        /// <summary>
        /// The id of the User. 
        /// </summary>
        public virtual int TemplateId {
            get; set;
        }

        /// <summary>
        /// The class name of the User inherited by all User types. 
        /// </summary>
        public virtual String ClassName {
            get {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// The assembly name of the User inherited by all User types. 
        /// </summary>
        public virtual String AssemblyName {
            get {
                return this.GetType().Namespace;
            }
        }

        public Template() {
        }

        /// <summary>
        /// The name of the Template for usage in the GUI. 
        /// </summary>
        public virtual String TemplateName {
            get; set;
        }

        /// <summary>
        /// The order in which to display the User items on the GUI. 
        /// </summary>
        public virtual int Order {
            get; set;
        }

        /// <summary>
        /// The Report Id which this User is tethered to. 
        /// </summary>
        public virtual int ReportId {
            get; set;
        }

        /// <summary>
        /// The Report object to which this User is attached. 
        /// </summary>
        public virtual Report Report {
            get; set;
        }
    }//end class

}//end namespace