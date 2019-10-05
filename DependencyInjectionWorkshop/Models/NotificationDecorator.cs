namespace DependencyInjectionWorkshop.Models
{
    public partial class NotificationDecorator : BaseAuthenticationDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(
            authentication)
        {
            _notification = notification;
        }

        public override bool Verify(string account, string inputPassword, string otp)
        {
            var isValid = base.Verify(account, inputPassword, otp);
            if (!isValid)
            {
                Notify(account);
            }

            return isValid;
        }

        private void Notify(string account)
        {
            _notification.Notify($"{account}: try to login failed");
        }
    }
}