using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace DigitalTransparencySystem.Helpers
{
    public static class MailSender
    {
        public static bool Send(string toEmail, string subject, string body)
        {
            try
            {
                string fromEmail = ConfigurationManager.AppSettings["SMTP_FromEmail"] ?? "noreply@dtas.edu";
                string fromPassword = ConfigurationManager.AppSettings["SMTP_Password"] ?? "";
                string smtpHost = ConfigurationManager.AppSettings["SMTP_Host"] ?? "smtp.gmail.com";
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_Port"] ?? "587");

                if (string.IsNullOrEmpty(fromPassword))
                {
                    System.Diagnostics.Debug.WriteLine("SMTP not configured. " + subject + " for " + toEmail);
                    return false;
                }

                var mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "DTAS");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email failed: " + ex.Message);
                return false;
            }
        }
    }
}
