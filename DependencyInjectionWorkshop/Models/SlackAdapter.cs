﻿using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter : INotification
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}