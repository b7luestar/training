using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string inputPassword, string otp)
        {
            if (GetIsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }
            var passwordFromDb = GetPassword(account);
            var hashPassword = HashPassword(inputPassword);
            var storedOtp = GetOtp(account);
            if (passwordFromDb == hashPassword && otp == storedOtp)
            {
                ResetFailCount(account);
                return true;
            }
            AddFailCount(account);
            LogFailCount(account, GetFailCount(account));
            NotifyUser(account);
            return false;
        }

        private static void NotifyUser(string account)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", $"{account} login failed", "my bot name");
        }

        private static void LogFailCount(string account, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }

        private int GetFailCount(string account)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private void AddFailCount(string account)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private void ResetFailCount(string account)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetOtp(string account)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var storedOtp = response.Content.ReadAsAsync<string>().Result;
            return storedOtp;
        }

        private static string HashPassword(string inputPassword)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }

        private string GetPassword(string account)
        {
            string password;
            using (var connection = new SqlConnection("my connection string"))
            {
                password = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return password;
        }

        private static bool GetIsAccountLocked(string account)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isAccountLocked;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}