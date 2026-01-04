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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public class AttachedFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }

    public interface IFileOperationProvider
    {
        /// <summary>
        /// Get data from files which user choose.
        /// </summary>
        /// <returns>Files content.</returns>
        Task<IEnumerable<AttachedFileInfo>> LoadFilesAsync();

        /// <summary>
        /// Store <paramref name="data"/> to file user choose.
        /// </summary>
        /// <param name="fileName">Name for file. User can change it.</param>
        Task SaveToFileAsync(byte[] data, string fileName);

        /// <summary>
        /// Store <paramref name="data"/> to temporary file and open it.
        /// </summary>
        /// <param name="fileName">Name for temporary file.</param>
        Task SaveToTempFileAndOpenAsync(byte[] data, string fileName);
    }
}
