namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        bool GetIsAccountLocked(string account);
        void AddFailCount(string account);
        int GetFailCount(string account);
        void ResetFailCount(string account);
    }
}