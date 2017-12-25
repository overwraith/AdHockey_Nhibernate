/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;

using AdHockey;

namespace AdHockey.Models {

    public class ForegotPasswordViewModel : IValidatableObject {
        /// <summary>
        /// User's email address. 
        /// </summary>
        public String EmailAddress {
            get; set;
        }

        /// <summary>
        /// First Password prompt. 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String PasswordA {
            get; set;
        }

        /// <summary>
        /// Second Password prompt. 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String PasswordB {
            get; set;
        }

        public ForegotPasswordViewModel() {
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            //validate that the passwords match
            if (String.CompareOrdinal(PasswordA, PasswordB) != 0)
                yield return new ValidationResult("Passwords do not match. ", new[] { "PasswordA", "PasswordB"});
        }//end method

    }//end class

}//end namespace