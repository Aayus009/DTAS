using System;
using System.IO;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class ViewEventAttachment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            int fileId;
            if (!int.TryParse(Request.QueryString["FileID"], out fileId))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            EventAttachmentRecord file = EventTaskService.GetAttachment(fileId);
            if (file == null || string.IsNullOrWhiteSpace(file.FilePath) || file.EventID <= 0)
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            EventAccess access = EventService.GetAccess(
                file.EventID,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string);
            if (access == null || !access.CanView)
            {
                Response.StatusCode = 403;
                Response.End();
                return;
            }

            string physical = EventTaskService.AttachmentPhysicalPath(file.FilePath, Server);
            if (string.IsNullOrEmpty(physical) || !File.Exists(physical))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            string ext = (Path.GetExtension(file.FileName ?? physical) ?? "").ToLowerInvariant();
            string contentType = "application/octet-stream";
            switch (ext)
            {
                case ".pdf": contentType = "application/pdf"; break;
                case ".png": contentType = "image/png"; break;
                case ".jpg":
                case ".jpeg": contentType = "image/jpeg"; break;
                case ".zip": contentType = "application/zip"; break;
                case ".doc": contentType = "application/msword"; break;
                case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
            }

            string downloadName = string.IsNullOrWhiteSpace(file.FileName) ? "work" + ext : Path.GetFileName(file.FileName);
            downloadName = downloadName.Replace("\"", "").Replace("\r", "").Replace("\n", "");
            if (string.IsNullOrWhiteSpace(downloadName))
                downloadName = "work" + ext;

            bool forceDownload = string.Equals(Request.QueryString["download"], "1", StringComparison.OrdinalIgnoreCase);
            string disposition = forceDownload || !CanPreviewInline(ext) ? "attachment" : "inline";

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", disposition + "; filename=\"" + downloadName + "\"");
            Response.AddHeader("Content-Length", new FileInfo(physical).Length.ToString());
            Response.TransmitFile(physical);
            Response.End();
        }

        private static bool CanPreviewInline(string ext)
        {
            return ext == ".pdf" || ext == ".png" || ext == ".jpg" || ext == ".jpeg";
        }
    }
}
