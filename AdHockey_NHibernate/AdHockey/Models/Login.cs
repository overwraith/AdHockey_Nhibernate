/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using AdHockey.Repositories;

namespace AdHockey.Models {
    /// <summary>
    /// The object associated with the login procedure. 
    /// </summary>
    public class Login {

        /// <summary>
        /// The User's Email Address. 
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public String EmailAddress {
            get; set;
        }//end property

        /// <summary>
        /// The User's password. 
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String Password {
            get; set;
        }//end property

        /// <summary>
        /// Login control can remember users when they log in next time. 
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe {
            get; set;
        }//end property

        /// <summary>
        /// Used to authenticate the user. 
        /// </summary>
        /// <returns></returns>
        public bool IsValid() {
            try {
                using (UserRepository usrRepo = new UserRepository()) {
                    usrRepo.BeginTransaction();
                    return usrRepo.IsValidLogin(this);
                }
            }
            catch (Exception ex) {
                return false;
            }
        }//end method

    }//end class

}//end namespace