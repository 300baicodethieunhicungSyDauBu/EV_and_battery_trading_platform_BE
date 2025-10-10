using System.Security.Cryptography;

namespace BE.REPOs.Service
{
    public interface IOTPService
    {
        string GenerateOTP();
        bool ValidateOTP(string otp);
    }

    public class OTPService : IOTPService
    {
        public string GenerateOTP()
        {
            // Generate 6-digit OTP
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public bool ValidateOTP(string otp)
        {
            // Validate OTP format (6 digits)
            return !string.IsNullOrEmpty(otp) && 
                   otp.Length == 6 && 
                   otp.All(char.IsDigit);
        }
    }
}
