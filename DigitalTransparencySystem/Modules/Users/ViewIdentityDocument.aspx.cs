using System;
using System.IO;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Users
{
    public partial class ViewIdentityDocument : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            int documentId;
            if (!int.TryParse(Request.QueryString["id"], out documentId))
            {
                Response.StatusCode = 400;
                Response.End();
                return;
            }

            IdentityDocumentInfo doc = IdentityDocumentService.GetById(documentId);
            if (doc == null)
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            bool allowed = RoleAccess.IsAdmin(Session["Role"] as string) || doc.UserID == userId;
            if (!allowed)
            {
                Response.StatusCode = 403;
                Response.End();
                return;
            }

            string physical = IdentityDocumentService.PhysicalPath(doc, Server);
            if (string.IsNullOrEmpty(physical) || !File.Exists(physical))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            string contentType = "application/octet-stream";
            switch ((doc.FileType ?? "").ToLowerInvariant())
            {
                case "pdf": contentType = "application/pdf"; break;
                case "png": contentType = "image/png"; break;
                case "jpg":
                case "jpeg": contentType = "image/jpeg"; break;
            }

            Response.Clear();
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", "inline; filename=\"" + (doc.OriginalFileName ?? "document") + "\"");
            Response.TransmitFile(physical);
            Response.End();
        }
    }
}
