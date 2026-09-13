using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace DigitalTransparencySystem.Helpers
{
    public class OtpIssueResult
    {
        public bool Sent { get; set; }
        public string Code { get; set; }
        public string Error { get; set; }
    }

    public static class EmailOtpService
    {
        public const string PurposeRegister = "Register";
        public const string PurposeRecovery = "Recovery";
        public const string PurposeEmailChange = "EmailChange";

        private const int ExpiryMinutes = 5;
        private const int MaxAttempts = 5;
        private const int ResendSeconds = 60;

        public static OtpIssueResult Issue(int userId, string email, string purpose)
        {
            var result = new OtpIssueResult();
            if (userId <= 0 || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(purpose))
            {
                result.Error = "Unable to send a verification code.";
                return result;
            }

            string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (var con = new SqlConnection(connStr))
            {
                con.Open();

                var recentCmd = new SqlCommand(
                    @"SELECT TOP 1 CreatedAt FROM EmailVerification
                      WHERE UserID = @UserID AND Purpose = @Purpose AND IsUsed = 0
                      ORDER BY CreatedAt DESC", con);
                recentCmd.Parameters.AddWithValue("@UserID", userId);
                recentCmd.Parameters.AddWithValue("@Purpose", purpose);
                object recent = recentCmd.ExecuteScalar();
                if (recent != null && recent != DBNull.Value)
                {
                    DateTime createdAt = Convert.ToDateTime(recent);
                    if (createdAt > DateTime.UtcNow.AddSeconds(-ResendSeconds))
                    {
                        result.Error = "Please wait a minute before requesting another code.";
                        return result;
                    }
                }

                string otp = GenerateOtp();
                result.Code = otp;

                var insert = new SqlCommand(
                    @"INSERT INTO EmailVerification (UserID, Email, OTP, Expiry, IsUsed, AttemptCount, Purpose, CreatedAt)
                      VALUES (@UserID, @Email, @OTP, @Expiry, 0, 0, @Purpose, GETUTCDATE())", con);
                insert.Parameters.AddWithValue("@UserID", userId);
                insert.Parameters.AddWithValue("@Email", email.Trim());
                insert.Parameters.AddWithValue("@OTP", otp);
                insert.Parameters.AddWithValue("@Expiry", DateTime.UtcNow.AddMinutes(ExpiryMinutes));
                insert.Parameters.AddWithValue("@Purpose", purpose);
                insert.ExecuteNonQuery();
            }

            string subject = "Your DTAS verification code";
            UserAccount account = AuthService.FindById(userId);
            MailContent mail = MailComposer.Build(
                null,
                "Your DTAS verification code is below. It expires in " + ExpiryMinutes + " minutes.",
                "If you did not request this, ignore this email.",
                null,
                null,
                null,
                MailComposer.FirstName(account != null ? account.FullName : null),
                result.Code);
            result.Sent = MailSender.Send(email.Trim(), subject, mail);
            return result;
        }

        public static bool Verify(int userId, string code, string purpose, out string error)
        {
            error = null;
            if (userId <= 0 || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(purpose))
            {
                error = "Enter the 6-digit code.";
                return false;
            }

            string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (var con = new SqlConnection(connStr))
            {
                con.Open();
                var select = new SqlCommand(
                    @"SELECT TOP 1 VerificationID, OTP, Expiry, AttemptCount, IsUsed
                      FROM EmailVerification
                      WHERE UserID = @UserID AND Purpose = @Purpose
                      ORDER BY CreatedAt DESC", con);
                select.Parameters.AddWithValue("@UserID", userId);
                select.Parameters.AddWithValue("@Purpose", purpose);

                int verificationId;
                string storedOtp;
                DateTime expiry;
                int attempts;
                bool used;
                using (SqlDataReader reader = select.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        error = "No verification code was found. Request a new one.";
                        return false;
                    }

                    verificationId = Convert.ToInt32(reader["VerificationID"]);
                    storedOtp = reader["OTP"].ToString();
                    expiry = Convert.ToDateTime(reader["Expiry"]);
                    attempts = Convert.ToInt32(reader["AttemptCount"]);
                    used = Convert.ToBoolean(reader["IsUsed"]);
                }

                if (used)
                {
                    error = "That code has already been used. Request a new one.";
                    return false;
                }

                if (attempts >= MaxAttempts)
                {
                    error = "Too many attempts. Request a new code.";
                    return false;
                }

                if (expiry <= DateTime.UtcNow)
                {
                    error = "That code has expired. Request a new one.";
                    return false;
                }

                if (!string.Equals(storedOtp, code.Trim(), StringComparison.Ordinal))
                {
                    var bump = new SqlCommand(
                        "UPDATE EmailVerification SET AttemptCount = AttemptCount + 1 WHERE VerificationID = @ID", con);
                    bump.Parameters.AddWithValue("@ID", verificationId);
                    bump.ExecuteNonQuery();
                    error = "Invalid or expired code. Please try again.";
                    return false;
                }

                var mark = new SqlCommand(
                    "UPDATE EmailVerification SET IsUsed = 1 WHERE VerificationID = @ID", con);
                mark.Parameters.AddWithValue("@ID", verificationId);
                mark.ExecuteNonQuery();
                return true;
            }
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            int atPos = email.IndexOf('@');
            if (atPos <= 1)
                return email;

            return email[0] + "****" + email.Substring(atPos - 1);
        }

        private static string GenerateOtp()
        {
            byte[] bytes = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);

            int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
            return (value % 900000 + 100000).ToString();
        }
    }
}
