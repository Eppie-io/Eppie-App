// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using System.Globalization;
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
                badgeElement.SetAttribute("value", num.ToString(CultureInfo.InvariantCulture));

                UpdateBadge(badgeXml);
            }
            catch (NotImplementedException)
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
