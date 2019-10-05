namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailCounter _FailCounter;

        public FailCounterDecorator(IAuthentication authentication, IFailCounter failCounter) : base(
            authentication)
        {
            _FailCounter = failCounter;
        }

        public override bool Verify(string account, string inputPassword, string otp)
        {
            CheckIsAccountLocked(account);
            var isValid = base.Verify(account, inputPassword, otp);
            if (isValid)
            {
                ResetFailCount(account);
            }
            else
            {
                AddFailCount(account);
            }

            return isValid;
        }

        private void ResetFailCount(string account)
        {
            _FailCounter.ResetFailCount(account);
        }

        private void AddFailCount(string account)
        {
            _FailCounter.AddFailCount(account);
        }
        private void CheckIsAccountLocked(string account)
        {
            if (_FailCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }
        }

    }

}