using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Wind
{
    public static class Email
    {
        /// <summary>
        /// Send an email on completion of a task.
        /// </summary>
        /// <param name="server">The SMTP server to connect.</param>
        /// <param name="port"></param>
        /// <param name="RecipientEmailAddress">The email address to send the the email.</param>
        /// <param name="SenderEmailAddress">The sender of the email.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        /// <returns>True if the email sent correctly. False if the email failed.</returns>
        /// <search>email, notification, send, recipient, sender</search>
        public static bool Send(string server, int port, string RecipientEmailAddress, string SenderEmailAddress, string subject, string body)
        {
            string to = RecipientEmailAddress;
            string from = SenderEmailAddress;
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            SmtpClient client = new SmtpClient();
            // Credentials are necessary if the server requires the client 
            // to authenticate before it will send e-mail on the client's behalf.
            try
            {
                client.Host = server;
                client.Port = port;
                client.UseDefaultCredentials = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Timeout = 10000;
                //client.Credentials = new System.Net.NetworkCredential(username, password);
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                            ex.ToString());
                return false;
            }
        }
    }
}
