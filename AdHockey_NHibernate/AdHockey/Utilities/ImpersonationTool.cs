/*Author: Cameron Block*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AdHockey.Utilities {

    public sealed class ImpersonationTool {
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private static WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        private static extern int LogonUserA(String lpszUserName, 
            String lpszDomain, 
            String lpszPassword, 
            int dwLogonType, 
            int dwLogonProvider, 
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr hToken, 
            int impersonationLevel, 
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        public static bool ImpersonateValidUser(String userName, String domain, String password) {
            WindowsIdentity tempWindowsIdentity;

            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf()) {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0) {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0) {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null) {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);

            return false;
        }//end method

        public static void UndoImpersonation() {
            impersonationContext.Undo();
        }//end method
    }//end class

}//end namespace