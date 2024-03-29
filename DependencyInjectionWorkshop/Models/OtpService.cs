﻿using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService : IOtpService
    {
        public string GetCurrentOtp(string account)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var storedOtp = response.Content.ReadAsAsync<string>().Result;
            return storedOtp;
        }
    }
}