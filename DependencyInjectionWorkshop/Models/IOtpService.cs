namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string GetOtp(string account);
    }
}