using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Configuration;

namespace AdHockey.Models {
    public class User {
        /// <summary>
        /// Unique identifier for group used by program and database layer. 
        /// </summary>
        public int UserId {
            get; set;
        }

        /// <summary>
        /// User's first name. 
        /// </summary>
        public String FirstName {
            get; set;
        }

        /// <summary>
        /// User's last name. 
        /// </summary>
        public String LastName {
            get; set;
        }

        /// <summary>
        /// Used to turn off user accessibility for instance when there are forensic investigations going on. 
        /// </summary>
        public bool IsActive {
            get; set;
        }

        /// <summary>
        /// The user's email address. 
        /// </summary>
        public String EmailAddress {
            get; set;
        }

        /// <summary>
        /// Telephone number of the user. 
        /// </summary>
        public String TelephoneNumber {
            get; set;
        }

        /// <summary>
        /// A hash of the users password is stored in the database and used for logins. 
        /// Storing a hash is more secure than storing plaintext. 
        /// No Company should have a comprehensive plaintext wordlist of it's users. 
        /// SHA-256 with a byte array storage size of 256
        /// </summary>
        public byte[] PasswordHash {
            get; set;
        }

        /// <summary>
        /// The groups a user is associated with. 
        /// </summary>
        public IList<Group> Groups {
            get; set;
        }

        /// <summary>
        /// The reports a user is scheduled to recieve. 
        /// </summary>
        public IList<Report> Reports {
            get; set;
        }

        public User() {
            IsActive = true;
        }

        public void SetPassword(String password) {
            //get salt from web config
            byte[] salt = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["salt"].ToString());
            byte[] passBytes = Encoding.UTF8.GetBytes(password);

            //perpend salt to password
            byte[] catPass = salt.Concat(passBytes).ToArray();

            //call all the hash algorithims here
            HashAlgorithm hashAlg = SHA512.Create();
            this.PasswordHash = hashAlg.ComputeHash(catPass);
        }//end method

        public bool ComparePassword(String password) {
            //get salt from web config
            byte[] salt = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["salt"].ToString());
            byte[] passBytes = Encoding.UTF8.GetBytes(password);

            //perpend salt to password
            byte[] catPass = salt.Concat(passBytes).ToArray();

            //call all the hash algorithims here
            HashAlgorithm hashAlg = SHA512.Create();

            byte[] incomingHash = hashAlg.ComputeHash(catPass);

            if (incomingHash.SequenceEqual(this.PasswordHash))
                return true;

            return false;
        }//end method

        public bool HasAccess(Report report) {
            foreach (var usr in report.Users)
                if (report.Users.Contains(usr))
                    return true;

            return false;
        }//end method
    }//end class

}//end namespace