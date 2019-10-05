﻿using System;
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
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            string password;
            using (var connection = new SqlConnection("my connection string"))
            {
                password = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();

            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var storedOtp = response.Content.ReadAsAsync<string>().Result;
            if (password == hashPassword && otp == storedOtp)
            {

                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

                resetResponse.EnsureSuccessStatusCode();
                return true;
            }
            else
            {
                
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;

                addFailedCountResponse.EnsureSuccessStatusCode();
                
                var failedCountResponse =
                    httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

                failedCountResponse.EnsureSuccessStatusCode();

                var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{account} failed times:{failedCount}");
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", $"{account} login failed", "my bot name");


                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}