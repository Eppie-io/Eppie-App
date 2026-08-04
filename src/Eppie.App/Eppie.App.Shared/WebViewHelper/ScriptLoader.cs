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

using System;

namespace Eppie.App.WebViewHelper
{
    public class ScriptLoader
    {
        static public string GetProtonCaptchaScript()
        {
            // JavaScript to C# communication (https://platform.uno/docs/articles/controls/WebView.html#javascript-to-c-communication)
            // The script (https://mail-api.proton.me/core/v4/captcha) uses the 'postMessageToParent' function to send messages to the parent.

            // Todo: Try to load the script from a embedded resource instead of hardcoding it here.

            return "function postWebViewMessage(message) {" + Environment.NewLine +
                   "  try {" + Environment.NewLine +
                   "    if (window.hasOwnProperty(\"chrome\") && typeof chrome.webview !== undefined) {" + Environment.NewLine +
                   "      // Windows" + Environment.NewLine +
                   "      chrome.webview.postMessage(message);" + Environment.NewLine +
                   "    } else if (window.hasOwnProperty(\"unoWebView\")) {" + Environment.NewLine +
                   "      // Android" + Environment.NewLine +
                   "      unoWebView.postMessage(message);" + Environment.NewLine +
                   "    } else if (window.hasOwnProperty(\"webkit\") && typeof webkit.messageHandlers !== undefined) {" + Environment.NewLine +
                   "      // linux, macOS, iOS" + Environment.NewLine +
                   "      webkit.messageHandlers.unoWebView.postMessage(message);" + Environment.NewLine +
                   "    } else {" + Environment.NewLine +
                   "      alert(\"Unknown message handler\");" + Environment.NewLine +
                   "    }" + Environment.NewLine +
                   "  } catch (ex) {" + Environment.NewLine +
                   "    alert(\"Error occurred: \" + ex);" + Environment.NewLine +
                   "  }" + Environment.NewLine +
                   "}" + Environment.NewLine +
                   "var postMessageToParent = postWebViewMessage;";
        }
    }
}
