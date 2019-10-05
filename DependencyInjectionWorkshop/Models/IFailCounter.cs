namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        bool IsAccountLocked(string account);
        void AddFailCount(string account);
        int GetFailCount(string account);
        void ResetFailCount(string account);
    }
}