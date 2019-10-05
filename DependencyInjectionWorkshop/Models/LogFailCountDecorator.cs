namespace DependencyInjectionWorkshop.Models
{
    public class LogFailCountDecorator:BaseAuthenticationDecorator
    {
        private readonly ILogger _Logger;
        private readonly IFailCounter _FailCounter;

        public LogFailCountDecorator(IAuthentication authentication, ILogger logger, IFailCounter failCounter):
            base(authentication)
        {
            _Logger = logger;
            _FailCounter = failCounter;
        }

        public override bool Verify(string account, string inputPassword, string otp)
        {
            var isValid = base.Verify(account, inputPassword, otp);
            if (!isValid)
            {
                LogFailedCount(account);
            }

            return isValid;
        }

        private void LogFailedCount(string account)
        {
            var failedCount = _FailCounter.GetFailCount(account);
            _Logger.LogInfo($"accountId:{account} failed times:{failedCount}");
        }
    }
}