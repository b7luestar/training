using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string _DefaultAccount = "joey";
        private const string _DefaultHashedPassword = "my hashed password";
        private const string _DefaultInputPassword = "abc";
        private const string _DefaultOtp = "123456";
        private const int _DefaultFailedCount = 91;
        private IAuthentication _AuthenticationService;
        private IFailCounter _FailedCounter;
        private IHashAdapter _Hash;
        private ILogger _Logger;
        private INotification _Notification;
        private IOtpService _OtpService;
        private IProfile _Profile;

        [SetUp]
        public void SetUp()
        {
            _Logger = Substitute.For<ILogger>();
            _FailedCounter = Substitute.For<IFailCounter>();
            _Notification = Substitute.For<INotification>();
            _OtpService = Substitute.For<IOtpService>();
            _Hash = Substitute.For<IHashAdapter>();
            _Profile = Substitute.For<IProfile>();
            _AuthenticationService =
                new AuthenticationService(_Profile, _Hash,_OtpService);

            _AuthenticationService = new FailCounterDecorator(_AuthenticationService, _FailedCounter);
            _AuthenticationService = new LogFailCountDecorator(_AuthenticationService,_Logger,_FailedCounter);
            _AuthenticationService = new NotificationDecorator(_AuthenticationService, _Notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);
            GivenHash(_DefaultInputPassword, _DefaultHashedPassword);
            GivenOtp(_DefaultAccount, _DefaultOtp);

            ShouldBeValid(_DefaultAccount, _DefaultInputPassword, _DefaultOtp);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);
            GivenHash(_DefaultInputPassword, _DefaultHashedPassword);
            GivenOtp(_DefaultAccount, _DefaultOtp);

            ShouldBeInvalid(_DefaultAccount, _DefaultInputPassword, "wrong otp");
        }

        [Test]
        public void should_notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(_DefaultAccount);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(_DefaultAccount);
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(_DefaultAccount);
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked(true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(_DefaultAccount, _DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(_DefaultAccount, _DefaultFailedCount);
        }

        private void LogShouldContains(string account, int failedCount)
        {
            _Logger.Received(1).LogInfo(
                Arg.Is<string>(m => m.Contains(account) && m.Contains(failedCount.ToString())));
        }

        private void GivenFailedCount(string account, int failedCount)
        {
            _FailedCounter.GetFailCount(account).Returns(failedCount);
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _AuthenticationService.Verify(_DefaultAccount, _DefaultInputPassword, _DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked(bool isLocked)
        {
            _FailedCounter.IsAccountLocked(_DefaultAccount).Returns(isLocked);
        }

        private void ShouldAddFailedCount(string account)
        {
            _FailedCounter.Received().AddFailCount(account);
        }

        private void ShouldResetFailedCount(string account)
        {
            _FailedCounter.Received(1).ResetFailCount(account);
        }

        private bool WhenValid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);
            GivenHash(_DefaultInputPassword, _DefaultHashedPassword);
            GivenOtp(_DefaultAccount, _DefaultOtp);

            var isValid = _AuthenticationService.Verify(_DefaultAccount, _DefaultInputPassword, _DefaultOtp);
            return isValid;
        }

        private void ShouldNotify(string account)
        {
            _Notification.Received(1).Notify(Arg.Is<string>(m => m.Contains(account)));
        }

        private bool WhenInvalid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);
            GivenHash(_DefaultInputPassword, _DefaultHashedPassword);
            GivenOtp(_DefaultAccount, _DefaultOtp);

            var isValid = _AuthenticationService.Verify(_DefaultAccount, _DefaultInputPassword, "wrong otp");
            return isValid;
        }

        private void ShouldBeInvalid(string account, string inputPassword, string otp)
        {
            var isValid = _AuthenticationService.Verify(account, inputPassword, otp);
            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(string account, string inputPassword, string otp)
        {
            var isValid = _AuthenticationService.Verify(account, inputPassword, otp);
            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string account, string otp)
        {
            _OtpService.GetCurrentOtp(account).Returns(otp);
        }

        private void GivenHash(string inputPassword, string hashedPassword)
        {
            _Hash.ComputeHash(inputPassword).Returns(hashedPassword);
        }

        private void GivenPassword(string account, string hashedPassword)
        {
            _Profile.GetPassword(account).Returns(hashedPassword);
        }
    }
}