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

using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ProtonAddressSettingsModel : BaseAddressSettingsModel
    {
        public ProtonAddressSettingsModel() : base()
        {
        }

        protected ProtonAddressSettingsModel(Account account) : base(account)
        {
        }

        public override Account ToAccount()
        {
            var account = base.ToAccount();
            if (account is null)
            {
                return null;
            }

            account.Type = MailBoxType.Proton;

            return account;
        }

        public static ProtonAddressSettingsModel Create(Account account)
        {
            return new ProtonAddressSettingsModel(account);
        }
    }
}
