using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace C2CNotificationFuncApp
{
    public static class SendNotifications
    {
        [FunctionName("c2cNotificationHubFunc")]
        public static void Run([ServiceBusTrigger("shanuka", "doctorimagingresultsubscription", AccessRights.Manage, Connection = "c2clabresultpushsb_RootManageSharedAccessKey_SERVICEBUS")]string mySbMsg, TraceWriter log)
        {
            NotificationHubClient hub = Notifications.Instance.Hub;
            log.Info($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
            if (!string.IsNullOrEmpty(mySbMsg))
            {
                LabResult deserializedResult = JsonConvert.DeserializeObject<LabResult>(mySbMsg);
                log.Info(string.Format("Practice Id: {0} , Doctor Id: {1} , LabResult Id: {2}", deserializedResult.PracticeId, deserializedResult.DoctorId, deserializedResult.LabResultId));
                string tag = CreateTag(deserializedResult);
                log.Info(string.Format("Tag is {0}", tag));
                tag = "shanuka";
                FormatMessageAndSendNotificationsAsync(hub, mySbMsg, tag, log);
            }
            else
            {
                log.Info($"Invalid Message from C2C Backend");
            }
        }
        static string CreateTag(LabResult deserializedResult)
        {
            return string.Format("{0}_{1}", deserializedResult.PracticeId, deserializedResult.DoctorId);
        }
        static async void FormatMessageAndSendNotificationsAsync(NotificationHubClient hub, string message, string tag, TraceWriter log)
        {
            string messageBody = null;
            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
            tag = "shanuka";
            log.Info("started format and send notifications async method");
            var registrations = await hub.GetRegistrationsByTagAsync(tag, 100);
            var distinctList = registrations.GroupBy(t => t.ETag).Select(s => s.First());
            log.Info(distinctList.Count().ToString());
            var Tags = new List<string> { tag };
            string label = null;
            foreach (var item in distinctList)
            {
                log.Info("started for loop");
                if (typeof(GcmRegistrationDescription) == item.GetType())
                {
                    messageBody = "{ \"data\" : {\"message\":" + message + "}}";
                    log.Info(messageBody);
                    label = "Gcm";
                    outcome = await hub.SendGcmNativeNotificationAsync(messageBody, Tags);
                }
                if (typeof(AppleRegistrationDescription) == item.GetType())
                {
                    messageBody = "{ \"aps\" : {\"alert\":" + message + "}}";
                    log.Info(messageBody);
                    label = "Aps";
                    outcome = await hub.SendAppleNativeNotificationAsync(messageBody, Tags);
                }
                if (outcome != null)
                {
                    log.Info(string.Format("{0} message is {1}", label, outcome.State.ToString()));
                }
            }
        }
    }
}