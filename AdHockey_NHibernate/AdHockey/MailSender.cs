/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net.Mail;

namespace AdHockey {

    /// <summary>
    /// Interface for sending emails. Override the Send method. 
    /// </summary>
    public interface IMailSender {
        void Send(String toAddress, String subject, String body, params MemoryStream[] attachments);
    }//end class

    /// <summary>
    /// Abstract class for sending emails. Overload the Send method. Implements some basic variables. 
    /// </summary>
    public abstract class AbstractMailSender : IMailSender {
        protected String FROM_ADDRESS = ConfigurationManager.AppSettings["ServerEmailAccount"];
        protected String PASSWORD = ConfigurationManager.AppSettings["ServerEmailPassword"];
        protected String HOST = ConfigurationManager.AppSettings["ServerEmailHost"];

        public AbstractMailSender() {
        }

        /// <summary>
        /// Send Email method overriden for differing functionality. 
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        public abstract void Send(string toAddress, string subject, string body, params MemoryStream[] attachments);
    }//end class

    /// <summary>
    /// Production mail sender program. 
    /// </summary>
    public class MailSender : AbstractMailSender {

        public MailSender() : base() {
        }

        /// <summary>
        /// Send an email to live email addresses, for usage in production. 
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        public override void Send(string toAddress, string subject, string body, params MemoryStream[] attachments) {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient();
            mail.From = new MailAddress(FROM_ADDRESS);
            mail.To.Add(toAddress);
            mail.Subject = subject;
            mail.Body = body;

            foreach (var stream in attachments) {
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(stream, 
                    new System.Net.Mime.ContentType("application / vnd.openxmlformats - officedocument.spreadsheetml.sheet"));
                mail.Attachments.Add(attachment);
            }

            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential(FROM_ADDRESS, PASSWORD);
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);
        }//end method
    }//end class

    /// <summary>
    /// Testing mail sender program. 
    /// </summary>
    public class TestingMailSender : AbstractMailSender {
        protected String TO_ADDRESS = "cnblock@cox.net";

        public TestingMailSender() : base() {
        }

        /// <summary>
        /// Send an email message to programmer's current workstation for testing purposes. 
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        public override void Send(string toAddress, string subject, string body, params MemoryStream[] attachments) {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient();
            mail.From = new MailAddress(FROM_ADDRESS);
            mail.To.Add(TO_ADDRESS);
            mail.Subject = subject;
            mail.Body = body;

            foreach (var stream in attachments) {
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(stream,
                    new System.Net.Mime.ContentType("application / vnd.openxmlformats - officedocument.spreadsheetml.sheet"));
                mail.Attachments.Add(attachment);
            }

            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential(FROM_ADDRESS, PASSWORD);
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);
        }//end method

    }//end class

}//end namespace