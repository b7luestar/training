using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _ProfileDao;
        private readonly IHashAdapter _Sha256Adapter;
        private readonly INotification _SlackAdapter;
        private readonly IOtpService _OtpService;
        private readonly IFailCounter _FailCounter;
        private readonly ILogger _NLogAdapter;

        public AuthenticationService()
        {
            _ProfileDao = new ProfileDao();
            _Sha256Adapter = new Sha256Adapter();
            _SlackAdapter = new SlackAdapter();
            _OtpService = new OtpService();
            _FailCounter = new FailCounter();
            _NLogAdapter = new NLogAdapter();
        }

        public AuthenticationService(IProfile profileDao, IHashAdapter sha256Adapter, INotification slackAdapter, IOtpService otpService, IFailCounter failCounter, ILogger nLogAdapter)
        {
            _ProfileDao = profileDao;
            _Sha256Adapter = sha256Adapter;
            _SlackAdapter = slackAdapter;
            _OtpService = otpService;
            _FailCounter = failCounter;
            _NLogAdapter = nLogAdapter;
        }

        public bool Verify(string account, string inputPassword, string otp)
        {
            if (_FailCounter.GetIsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }
            var passwordFromDb = _ProfileDao.GetPassword(account);
            var hashPassword = _Sha256Adapter.ComputeHash(inputPassword);
            var storedOtp = _OtpService.GetOtp(account);
            if (passwordFromDb == hashPassword && otp == storedOtp)
            {
                _FailCounter.ResetFailCount(account);
                return true;
            }

            _FailCounter.AddFailCount(account);
            LogFailCount(account);
            _SlackAdapter.Nodify($"{account}: try to login failed");
            return false;
        }

        private void LogFailCount(string account)
        {
            var failedCount = _FailCounter.GetFailCount(account);
            _NLogAdapter.LogInfo($"accountId:{account} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}