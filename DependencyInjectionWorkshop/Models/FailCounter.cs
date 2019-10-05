using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailCounter
    {
        public bool GetIsAccountLocked(string account)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }

        public void AddFailCount(string account)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int GetFailCount(string account)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            return failedCountResponse.Content.ReadAsAsync<int>().Result;
        }

        public void ResetFailCount(string account)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

            resetResponse.EnsureSuccessStatusCode();
        }
    }
}