namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string GetCurrentOtp(string account);
    }
}