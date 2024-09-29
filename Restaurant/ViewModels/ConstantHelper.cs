namespace Restaurant.ViewModels
{
    public class ConstantHelper
    {
        public string hostemail { get; } = "smtp.gmail.com";
        public int port { get; } = 587;
        public string emailsender { get; } = "longps704@gmail.com";
        public string paswordsender { get; } = "coqdualrvdophife";

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
