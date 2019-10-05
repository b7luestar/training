namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IHashAdapter _Hash;
        private readonly IOtpService _OtpService;
        private readonly IProfile _Profile;

        public AuthenticationService()
        {
            _Profile = new ProfileDao();
            _Hash = new Sha256Adapter();
            _OtpService = new OtpService();
        }

        public AuthenticationService(IProfile profile, IHashAdapter hash, IOtpService otpService)
        {
            _Profile = profile;
            _Hash = hash;
            _OtpService = otpService;
        }

        public bool Verify(string account, string inputPassword, string otp)
        {
            var passwordFromDb = _Profile.GetPassword(account);

            var hashedPassword = _Hash.ComputeHash(inputPassword);

            var currentOtp = _OtpService.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            return false;
        }
    }
}