using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {

    /// <summary>
    /// This class is a bridge entity, will not have to use in any of the GUI Code. 
    /// </summary>
    public class ReportUserBridge {
        /// <summary>
        /// Unique identifier for connecting to the report table. 
        /// </summary>
        public int ReportId {
            get; set;
        }

        /// <summary>
        /// Unique identifier for connecting to the user table. 
        /// </summary>
        public int UserId {
            get; set;
        }

        public override int GetHashCode() {
            return ReportId.GetHashCode() ^ RotateLeft(UserId.GetHashCode(), 16);
        }

        public int RotateLeft(int value, int count) {
            return (value << count) | (value >> (32 - count));
        }

        public override bool Equals(object obj) {
            return this.GetHashCode().Equals(obj.GetHashCode());
        }
    }//end class

}//end namespace