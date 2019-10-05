using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        private readonly Logger _Logger;

        public NLogAdapter()
        {
            _Logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void LogInfo(string message)
        {
            _Logger.Info(message);
        }
    }
}