using System;

namespace DigitalTransparencySystem
{
    public partial class Contact : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize form
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Handle form submission
            string fullName = txtFullName.Text;
            string email = txtEmail.Text;
            string category = ddlCategory.SelectedValue;
            string statement = txtStatement.Text;
            bool consent = chkConsent.Checked;

            // TODO: Validate and save to database
            // TODO: Send confirmation email

            // Show success message
            // You can use a Label control or JavaScript alert
        }

        protected void btnSignUp_Click(object sender, EventArgs e)
        {
            // Handle newsletter signup
            string email = txtNewsletter.Text;

            // TODO: Validate email and add to mailing list
        }
    }
}