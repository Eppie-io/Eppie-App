using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Tuvi.App.Shared.Services
{
    public static class NotificationService
    {
        public static void SetBadgeNumber(int num)
        {
            try
            {
                // Get the blank badge XML payload for a badge number
                var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);  // ToDo: Uno0001

                // Set the value of the badge in the XML to our number
                var badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
                badgeElement.SetAttribute("value", num.ToString());

                UpdateBadge(badgeXml);
            }
            catch(NotImplementedException)
            {
                //ignore for now
            }
        }

        public static void SetBadgeGlyph(string badgeGlyphValue = "newMessage")
        {
            // Get the blank badge XML payload for a badge glyph
            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph); // ToDo: Uno0001

            // Set the value of the badge in the XML to our glyph value
            var badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);

            UpdateBadge(badgeXml);
        }

        public static void ShowToastNotification(string message)
        {            
            var xml = $@"<toast>
    <visual>
        <binding template='ToastGeneric'>
            <text>{message}</text>
        </binding>
    </visual>
</toast>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var notification = new ToastNotification(doc);

            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(notification); //ToDo: Uno0001
            }
            catch (NotImplementedException)
            {
                //ignore for now
            }
        }

        private static void UpdateBadge(XmlDocument badgeXml)
        {
            // Create the badge notification
            var badge = new BadgeNotification(badgeXml); // ToDo: Uno0001

            // Create the badge updater for the application
            var badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication(); // ToDo: Uno0001

            // And update the badge
            badgeUpdater.Update(badge); // ToDo: Uno0001
        }
    }
}
