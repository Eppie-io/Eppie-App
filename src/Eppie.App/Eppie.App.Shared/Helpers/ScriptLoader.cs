// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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

namespace Eppie.App.Helpers
{
    public class ScriptLoader
    {
        static public string GetProtonCaptchaScript()
        {
            // JavaScript to C# communication (https://platform.uno/docs/articles/controls/WebView.html#javascript-to-c-communication)
            // The script (https://mail-api.proton.me/core/v4/captcha) uses the 'postMessageToParent' function to send messages to the parent.

            // Todo: Try to load the script from a embedded resource instead of hardcoding it here.

            return @"
                    function postWebViewMessage(message) {
                        try {
                            if (window.hasOwnProperty('chrome') && typeof chrome.webview !== 'undefined') {
                                // Windows
                                chrome.webview.postMessage(message);
                            } else if (window.hasOwnProperty('unoWebView')) {
                                // Android
                                unoWebView.postMessage(typeof message === 'string' ? message : JSON.stringify(message));
                            } else if (window.hasOwnProperty('webkit') && typeof webkit.messageHandlers !== 'undefined' && typeof webkit.messageHandlers.unoWebView !== 'undefined' ) {
                                // linux, macOS, iOS
                                webkit.messageHandlers.unoWebView.postMessage(typeof message === 'string' ? message : JSON.stringify(message));
                            } else {
                                alert('Unknown message handler');
                            }
                        } catch (ex) {
                            alert('Error occurred: ' + ex);
                        }
                    }
                    var postMessageToParent = postWebViewMessage;";
        }
    }
}
