using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public partial class Contact : System.Web.UI.Page
    {
        private const string Inbox = "edu.transparency@gmail.com";

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string fullName = (txtFullName.Text ?? "").Trim();
            string email = (txtEmail.Text ?? "").Trim();
            string category = ddlCategory.SelectedValue ?? "";
            string statement = (txtStatement.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(statement))
            {
                UiNotice.Bind(lblNotice, "Fill in your name, email, how we can help, and your question.", null);
                return;
            }

            if (!chkConsent.Checked)
            {
                UiNotice.Bind(lblNotice, "Please confirm that DTAS may use this message to reply.", null);
                return;
            }

            string topic = ddlCategory.SelectedItem != null ? ddlCategory.SelectedItem.Text : category;
            string subject = "DTAS inquiry: " + topic;
            string body =
                "Name: " + fullName + Environment.NewLine +
                "Email: " + email + Environment.NewLine +
                "Topic: " + topic + Environment.NewLine + Environment.NewLine +
                statement;

            bool sent = MailSender.Send(Inbox, subject, body);
            if (!sent)
            {
                UiNotice.Bind(lblNotice,
                    "We could not send the message just now. Email " + Inbox + " or call 9814524825.",
                    null);
                return;
            }

            txtFullName.Text = "";
            txtEmail.Text = "";
            ddlCategory.SelectedIndex = 0;
            txtStatement.Text = "";
            chkConsent.Checked = false;
            UiNotice.Bind(lblNotice, null, "Thanks. We received your inquiry.");
        }
    }
}
