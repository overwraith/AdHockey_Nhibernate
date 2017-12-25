using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Models {

    /// <summary>
    /// This class is a bridge entity, will not have to use in any of the GUI Code. 
    /// </summary>
    public class GroupUserBridge {
        /// <summary>
        /// Unique identifier for linking to the group table. 
        /// </summary>
        public int GroupId {
            get; set;
        }

        /// <summary>
        /// Unique identifier for linking to the user table. 
        /// </summary>
        public int UserId {
            get; set;
        }

        public override int GetHashCode() {
            return GroupId.GetHashCode() ^ RotateLeft(UserId.GetHashCode(), 16);
        }

        public int RotateLeft(int value, int count) {
            return (value << count) | (value >> (32 - count));
        }

        public override bool Equals(object obj) {
            return this.GetHashCode().Equals(obj.GetHashCode());
        }
    }//end class

}//end namespace