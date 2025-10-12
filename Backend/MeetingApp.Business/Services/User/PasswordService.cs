using MeetingApp.Business.Abstractions.User;
using BC = BCrypt.Net.BCrypt;

namespace MeetingApp.Business.Services.User
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BC.HashPassword(password, BC.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BC.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(c => !char.IsLetterOrDigit(c))) return false;

            return true;
        }
    }
}
