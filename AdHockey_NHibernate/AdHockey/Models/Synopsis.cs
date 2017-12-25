/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdHockey.Models {

    /// <summary>
    /// Object for adding attribution for the reports that are being inserted into the reporting engine. 
    /// </summary>
    [Bind(Include = "RecommenderFirstName,RecommenderLastName,ReccomenderTelephoneNum,ImplementerFirstName,ImplementerLastName,ImplementerTelephoneNum,DateCreated,BusinessCase")]
    public class Synopsis {

        /// <summary>
        /// Unique identifier for the synopsis table. 
        /// </summary>
        public int SynopsisId {
            get; set;
        }

        /// <summary>
        /// Last name of person who reccomended the creation of the report. 
        /// </summary>
        public String RecommenderFirstName {
            get; set;
        }

        /// <summary>
        /// Last name of person who reccomended the creation of the report. 
        /// </summary>
        public String RecommenderLastName {
            get; set;
        }

        /// <summary>
        /// Telephone number of the person who reccomended the creation of the report. 
        /// </summary>
        public String ReccomenderTelephoneNum {
            get; set;
        }

        /// <summary>
        /// First name of the person who inserted the report into the engine. 
        /// </summary>
        public String ImplementerFirstName {
            get; set;
        }

        /// <summary>
        /// Last name of the person who inserted the report into the engine. 
        /// </summary>
        public String ImplementerLastName {
            get; set;
        }

        /// <summary>
        /// Telephone number of the person who inserted the report into the engine. 
        /// </summary>
        public String ImplementerTelephoneNum {
            get; set;
        }

        /// <summary>
        /// Date the synopsis record was created. 
        /// </summary>
        public DateTime DateCreated {
            get; set;
        }

        /// <summary>
        /// Detailed record of the people involved in the creation of the report. 
        /// </summary>
        public String BusinessCase {
            get; set;
        }

        /// <summary>
        /// The Unique identifier for the report this synopsis record describes. 
        /// </summary>
        public int ReportId {
            get; set;
        }

        /// <summary>
        /// The Report object to which this report object is attached to. 
        /// </summary>
        public Report Report {
            get; set;
        }
    }//end class

}//end namespace

