using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class IdentityDocumentInfo
    {
        public int DocumentID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string VerificationStatus { get; set; }
        public string RejectionReason { get; set; }
        public string InstitutionalID { get; set; }
        public string UserVerificationStatus { get; set; }
        public bool IdentityVerified { get; set; }
    }

    public static class IdentityDocumentService
    {
        public const int MaxBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "pdf"
        };

        public static string ValidateUpload(HttpPostedFile file)
        {
            if (file == null || file.ContentLength <= 0)
                return "Choose a JPG, PNG, or PDF file.";

            if (file.ContentLength > MaxBytes)
                return "File must be 5 MB or smaller.";

            string ext = NormalizeExt(Path.GetExtension(file.FileName));
            if (!AllowedTypes.Contains(ext))
                return "Only JPG, PNG, and PDF files are accepted.";

            return null;
        }

        public static IdentityDocumentInfo GetCurrent(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 d.DocumentID, d.UserID, d.FileName, d.OriginalFileName, d.FilePath,
                         d.FileType, d.FileSize, d.UploadDate, d.VerificationStatus, d.RejectionReason,
                         u.FullName, u.Email, u.InstitutionalID,
                         ISNULL(u.VerificationStatus, N'None') AS UserVerificationStatus,
                         ISNULL(u.IdentityVerified, 0) AS IdentityVerified,
                         r.RoleName
                  FROM IdentityDocuments d
                  INNER JOIN Users u ON u.UserID = d.UserID
                  INNER JOIN Roles r ON r.RoleID = u.RoleID
                  WHERE d.UserID = @UserID AND d.IsCurrent = 1
                  ORDER BY d.DocumentID DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadDoc(reader);
                }
            }
        }

        public static IdentityDocumentInfo GetById(int documentId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT d.DocumentID, d.UserID, d.FileName, d.OriginalFileName, d.FilePath,
                         d.FileType, d.FileSize, d.UploadDate, d.VerificationStatus, d.RejectionReason,
                         u.FullName, u.Email, u.InstitutionalID,
                         ISNULL(u.VerificationStatus, N'None') AS UserVerificationStatus,
                         ISNULL(u.IdentityVerified, 0) AS IdentityVerified,
                         r.RoleName
                  FROM IdentityDocuments d
                  INNER JOIN Users u ON u.UserID = d.UserID
                  INNER JOIN Roles r ON r.RoleID = u.RoleID
                  WHERE d.DocumentID = @DocumentID", con))
            {
                cmd.Parameters.AddWithValue("@DocumentID", documentId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadDoc(reader);
                }
            }
        }

        public static DataTable ListQueue(string status)
        {
            string filter = string.Equals(status, "All", StringComparison.OrdinalIgnoreCase)
                ? ""
                : "AND d.VerificationStatus = @Status";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT d.DocumentID, d.UserID, u.FullName, u.Email, r.RoleName,
                         u.InstitutionalID, d.OriginalFileName, d.FileType, d.FileSize,
                         d.UploadDate, d.VerificationStatus, d.RejectionReason,
                         ISNULL(u.VerificationStatus, N'None') AS UserVerificationStatus
                  FROM IdentityDocuments d
                  INNER JOIN Users u ON u.UserID = d.UserID
                  INNER JOIN Roles r ON r.RoleID = u.RoleID
                  WHERE d.IsCurrent = 1 " + filter + @"
                  ORDER BY
                    CASE d.VerificationStatus WHEN N'Pending' THEN 0 WHEN N'Rejected' THEN 1 ELSE 2 END,
                    d.UploadDate DESC", con))
            {
                if (!string.IsNullOrEmpty(filter))
                    cmd.Parameters.AddWithValue("@Status", status);

                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static int CountPending()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM IdentityDocuments
                  WHERE IsCurrent = 1 AND VerificationStatus = N'Pending'", con))
            {
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static string SaveUpload(int userId, HttpPostedFile file, string institutionalId, HttpServerUtility server)
        {
            string validation = ValidateUpload(file);
            if (validation != null)
                return validation;

            string ext = NormalizeExt(Path.GetExtension(file.FileName));
            string storedName = Guid.NewGuid().ToString("N") + "." + ext;
            string relativeDir = "~/Uploads/IdentityDocuments/" + userId + "/";
            string physicalDir = server.MapPath(relativeDir);
            Directory.CreateDirectory(physicalDir);

            string physicalPath = Path.Combine(physicalDir, storedName);
            file.SaveAs(physicalPath);

            string relativePath = relativeDir.TrimEnd('/') + "/" + storedName;
            string original = Path.GetFileName(file.FileName);
            if (original.Length > 280)
                original = original.Substring(original.Length - 280);

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        var clear = new SqlCommand(
                            "UPDATE IdentityDocuments SET IsCurrent = 0 WHERE UserID = @UserID AND IsCurrent = 1", con, tx);
                        clear.Parameters.AddWithValue("@UserID", userId);
                        clear.ExecuteNonQuery();

                        var insert = new SqlCommand(
                            @"INSERT INTO IdentityDocuments
                                (UserID, FileName, OriginalFileName, FilePath, FileType, FileSize, UploadDate, VerificationStatus, IsCurrent)
                              VALUES
                                (@UserID, @FileName, @OriginalFileName, @FilePath, @FileType, @FileSize, GETDATE(), N'Pending', 1)", con, tx);
                        insert.Parameters.AddWithValue("@UserID", userId);
                        insert.Parameters.AddWithValue("@FileName", storedName);
                        insert.Parameters.AddWithValue("@OriginalFileName", original);
                        insert.Parameters.AddWithValue("@FilePath", relativePath);
                        insert.Parameters.AddWithValue("@FileType", ext);
                        insert.Parameters.AddWithValue("@FileSize", file.ContentLength);
                        insert.ExecuteNonQuery();

                        var user = new SqlCommand(
                            @"UPDATE Users
                              SET VerificationStatus = N'Pending',
                                  IdentityVerified = 0,
                                  RejectionReason = NULL,
                                  InstitutionalID = @InstitutionalID
                              WHERE UserID = @UserID", con, tx);
                        user.Parameters.AddWithValue("@UserID", userId);
                        user.Parameters.AddWithValue("@InstitutionalID",
                            string.IsNullOrWhiteSpace(institutionalId) ? (object)DBNull.Value : institutionalId.Trim());
                        user.ExecuteNonQuery();

                        NotifyAdmins(con, tx, userId);
                        tx.Commit();
                    }
                    catch (SqlException ex)
                    {
                        tx.Rollback();
                        try { File.Delete(physicalPath); } catch { }
                        if (ex.Number == 2601 || ex.Number == 2627)
                            return "That institutional ID is already in use.";
                        return "Could not save the document. Try again.";
                    }
                    catch
                    {
                        tx.Rollback();
                        try { File.Delete(physicalPath); } catch { }
                        return "Could not save the document. Try again.";
                    }
                }
            }

            AuthService.WriteAudit(userId, "IdentitySubmitted", "User", userId, "Identity document uploaded.", null);
            return null;
        }

        public static string Decide(int userId, int adminId, bool approve, string rejectionReason)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("sp_VerifyUserIdentity", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@AdminID", adminId);
                cmd.Parameters.AddWithValue("@Decision", approve ? "Approved" : "Rejected");
                cmd.Parameters.AddWithValue("@RejectionReason",
                    approve ? (object)DBNull.Value : (object)(rejectionReason ?? ""));
                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    return ex.Message;
                }
            }

            if (approve)
            {
                NotificationService.Send(userId, "ID accepted",
                    "Your institutional ID was verified. DTAS features are now unlocked.",
                    "Identity", userId, "User");
                MailSender.SendToUser(userId, "Your ID was verified",
                    MailComposer.Build(
                        null,
                        "Your institutional ID was accepted. You can use DTAS normally now.",
                        null,
                        null,
                        "Open DTAS",
                        MailSender.AbsoluteUrl("~/Modules/Dashboard/UsersDashboard.aspx")));
            }
            else
            {
                string reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "Upload a clearer photo or PDF of your institutional ID and submit again."
                    : rejectionReason.Trim();
                NotificationService.Send(userId, "ID rejected",
                    string.IsNullOrWhiteSpace(rejectionReason)
                        ? "Your institutional ID was not accepted. Upload a clearer document."
                        : "Your institutional ID was not accepted. Reason: " + rejectionReason.Trim(),
                    "Identity", userId, "User");
                MailSender.SendToUser(userId, "Your ID was not accepted",
                    MailComposer.Build(
                        null,
                        "Your institutional ID was not accepted.",
                        reason,
                        null,
                        "Upload another document",
                        MailSender.AbsoluteUrl("~/Modules/Settings/IdentityVerification.aspx")));
            }

            AuthService.WriteAudit(adminId, approve ? "IdentityApproved" : "IdentityRejected", "User", userId, rejectionReason, null);
            return null;
        }

        public static string PhysicalPath(IdentityDocumentInfo doc, HttpServerUtility server)
        {
            if (doc == null || string.IsNullOrEmpty(doc.FilePath))
                return null;
            return server.MapPath(doc.FilePath);
        }

        private static void NotifyAdmins(SqlConnection con, SqlTransaction tx, int userId)
        {
            var notify = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  SELECT u.UserID,
                         N'Identity document pending',
                         N'A community member submitted an ID for review.',
                         0, N'Identity', @RelatedID, N'IdentityDocument', GETDATE()
                  FROM Users u
                  WHERE u.RoleID = 1 AND ISNULL(u.IsDeleted, 0) = 0 AND u.IsActive = 1 AND u.UserID <> @RelatedID", con, tx);
            notify.Parameters.AddWithValue("@RelatedID", userId);
            notify.ExecuteNonQuery();
        }

        private static string NormalizeExt(string ext)
        {
            if (string.IsNullOrEmpty(ext))
                return "";
            return ext.Trim().TrimStart('.').ToLowerInvariant();
        }

        private static IdentityDocumentInfo ReadDoc(SqlDataReader reader)
        {
            return new IdentityDocumentInfo
            {
                DocumentID = Convert.ToInt32(reader["DocumentID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                FileName = reader["FileName"].ToString(),
                OriginalFileName = reader["OriginalFileName"].ToString(),
                FilePath = reader["FilePath"].ToString(),
                FileType = reader["FileType"].ToString(),
                FileSize = Convert.ToInt32(reader["FileSize"]),
                UploadDate = Convert.ToDateTime(reader["UploadDate"]),
                VerificationStatus = reader["VerificationStatus"].ToString(),
                RejectionReason = reader["RejectionReason"] == DBNull.Value ? null : reader["RejectionReason"].ToString(),
                FullName = reader["FullName"].ToString(),
                Email = reader["Email"].ToString(),
                InstitutionalID = reader["InstitutionalID"] == DBNull.Value ? null : reader["InstitutionalID"].ToString(),
                UserVerificationStatus = reader["UserVerificationStatus"].ToString(),
                IdentityVerified = Convert.ToBoolean(reader["IdentityVerified"]),
                Role = reader["RoleName"].ToString()
            };
        }
    }
}
