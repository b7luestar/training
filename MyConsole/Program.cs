using System;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    
 class Program
    {
        private static ILogger _Logger;
        private static IFailCounter _FailCounter;
        private static INotification _Notification;
        private static IOtpService _OtpService;
        private static IHashAdapter _Hash;
        private static IProfile _Profile;
        private static IAuthentication _AuthenticationService;
        private static IContainer _Container;

        static void Main(string[] args)
        {
            RegisterContainer();
            /*_Logger = new FakeLogger();
            _FailCounter = new FakeFailCounter();
            _Notification = new FakeSlack();
            _OtpService = new FakeOtp();
            _Hash = new FakeHash();
            _Profile = new FakeProfile();
            _AuthenticationService =
                new AuthenticationService(_Profile, _Hash, _OtpService);

            _AuthenticationService = new FailCounterDecorator(_AuthenticationService, _FailCounter);
            _AuthenticationService = new LogFailCountDecorator(_AuthenticationService, _Logger, _FailCounter);
            _AuthenticationService = new NotificationDecorator(_AuthenticationService, _Notification);*/
            _AuthenticationService = _Container.Resolve<IAuthentication>();

            var isValid = _AuthenticationService.Verify("joey","abc", "wrong otp");
            Console.WriteLine($"result is {isValid}");
            Console.Read();
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeFailCounter>().As<IFailCounter>();
            builder.RegisterType<FakeHash>().As<IHashAdapter>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterType<FailCounterDecorator>();
            builder.RegisterType<LogFailCountDecorator>();
            builder.RegisterType<NotificationDecorator>();

            builder.RegisterDecorator<FailCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogFailCountDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            _Container = builder.Build();
        }
    }

   internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void LogInfo(string message)
        {
            Info(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Send(string message)
        {
            PushMessage(message);
        }

        public void Notify(string message)
        {
            PushMessage(message);
        }
    }

    internal class FakeFailCounter : IFailCounter
    {
        public void ResetFailCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(ResetFailCount)}({accountId})");
        }

        public void AddFailCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(AddFailCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(GetFailCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHashAdapter
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }

        public string ComputeHash(string input)
        {
            return Compute(input);
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }

        public string GetPasswordFromDb(string account)
        {
            return GetPassword(account);
        }
    }
}
