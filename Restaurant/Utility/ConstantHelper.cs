namespace Restaurant.Utility
{
    public class ConstantHelper
    {
        public string hostemail { get; } = "smtp.gmail.com";
        public int port { get; } = 587;
        public string emailsender { get; } = "";
        public string paswordsender { get; } = "";

        public string GenerateOTP()
        {
            Random random = new Random();
            int otpLength = 6; // Length of the OTP
            string otp = "";

            for (int i = 0; i < otpLength; i++)
            {
                otp += random.Next(0, 9).ToString();
            }

            return otp;
        }
    }
}
