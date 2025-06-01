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

using Serilog.Enrichers.Sensitive;

namespace Eppie.App.Shared.Logging
{
    public class HashTransformOperator<TBaseMaskingOperator> : IMaskingOperator
        where TBaseMaskingOperator : IMaskingOperator, new()
    {
        private TBaseMaskingOperator BaseOperator { get; } = new TBaseMaskingOperator();

        public MaskingResult Mask(string input, string mask)
        {
            MaskingResult result = BaseOperator.Mask(input, mask);

            if (result.Match)
            {
                result.Result = string.Concat("#", input.GetHashCode().ToString());
            }

            return result;
        }
    }
}
