using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2CNotificationFuncApp
{
    public class Notifications
    {
        public static Notifications Instance = new Notifications();
        public NotificationHubClient Hub { get; set; }
        string hubConnectionString = ConfigurationManager.AppSettings["c2cnotificationhubconnectionstring"];
        string hubName = ConfigurationManager.AppSettings["c2cnotificationhubname"];

        private Notifications()
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString(hubConnectionString,
                                                                         hubName);
        }
    }
}
