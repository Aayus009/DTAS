using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public static class MailSender
    {
        private static string lastKnownBaseUrl;

        public static string AbsoluteUrl(string appRelativePath)
        {
            string path = (appRelativePath ?? "~/").Trim();
            if (path.StartsWith("~/"))
                path = path.Substring(2);
            path = path.TrimStart('/');

            string baseUrl = ConfigurationManager.AppSettings["DTAS_PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl) && HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                string app = HttpContext.Current.Request.ApplicationPath ?? "/";
                if (!app.EndsWith("/"))
                    app += "/";
                baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + app;
                lastKnownBaseUrl = baseUrl;
            }
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = lastKnownBaseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl))
                return "~/" + path;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";
            return baseUrl + path;
        }

        public static bool SendToUser(int userId, string subject, MailContent content)
        {
            UserAccount user = AuthService.FindById(userId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return false;
            return Send(user.Email, subject, content);
        }

        public static bool Send(string toEmail, string subject, MailContent content)
        {
            if (content == null)
                return false;
            return Deliver(toEmail, subject, content);
        }

        public static bool Send(string toEmail, string subject, string body)
        {
            return Deliver(toEmail, subject, MailComposer.WrapPlain(subject, body));
        }

        private static bool Deliver(string toEmail, string subject, MailContent content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                    return false;

                string fromEmail = ConfigurationManager.AppSettings["SMTP_FromEmail"] ?? "noreply@dtas.edu";
                string fromPassword = ConfigurationManager.AppSettings["SMTP_Password"] ?? "";
                string smtpHost = ConfigurationManager.AppSettings["SMTP_Host"] ?? "smtp.gmail.com";
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_Port"] ?? "587");

                if (string.IsNullOrEmpty(fromPassword))
                {
                    System.Diagnostics.Debug.WriteLine("SMTP not configured. " + subject + " for " + toEmail);
                    return false;
                }

                string plain = string.IsNullOrWhiteSpace(content.Plain) ? (subject ?? "DTAS") : content.Plain;
                string html = string.IsNullOrWhiteSpace(content.Html) ? MailComposer.WrapPlain(subject, plain).Html : content.Html;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "DTAS");
                    mail.To.Add(toEmail.Trim());
                    mail.Subject = subject ?? "DTAS";
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.SubjectEncoding = Encoding.UTF8;

                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(plain, Encoding.UTF8, MediaTypeNames.Text.Plain);
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);
                    mail.AlternateViews.Add(plainView);
                    mail.AlternateViews.Add(htmlView);

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Timeout = 8000;
                        smtp.Send(mail);
                    }
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
