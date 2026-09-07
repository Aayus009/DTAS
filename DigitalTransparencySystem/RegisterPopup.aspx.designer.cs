namespace DigitalTransparencySystem
{
    public partial class RegisterPopup
    {
        protected global::System.Web.UI.WebControls.Panel pnlRegister;
        protected global::System.Web.UI.WebControls.LinkButton btnGoogleLogin;
        protected global::System.Web.UI.WebControls.TextBox txtFullName;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvFullName;
        protected global::System.Web.UI.WebControls.TextBox txtEmail;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEmail;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator revEmail;
        protected global::System.Web.UI.WebControls.DropDownList ddlRole;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvRole;
        protected global::System.Web.UI.WebControls.TextBox txtPassword;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvPassword;
        protected global::System.Web.UI.WebControls.TextBox txtConfirmPassword;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvConfirmPassword;
        protected global::System.Web.UI.WebControls.CompareValidator cvPassword;
        protected global::System.Web.UI.WebControls.CheckBox chkCharter;
        protected global::System.Web.UI.WebControls.CustomValidator cvCharter;
        protected global::System.Web.UI.WebControls.Button btnRegister;
        protected global::System.Web.UI.WebControls.Panel pnlMessage;
        protected global::System.Web.UI.WebControls.Label lblMessage;
        protected global::System.Web.UI.WebControls.Panel pnlOTP;
        protected global::System.Web.UI.WebControls.Label lblOtpSentTo;
        protected global::System.Web.UI.WebControls.TextBox txtOTP;
        protected global::System.Web.UI.WebControls.Button btnVerifyOTP;
        protected global::System.Web.UI.WebControls.Button btnResendOTP;
    }
}
