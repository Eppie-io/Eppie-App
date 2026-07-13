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
using System.IO;

namespace Eppie.App.Common
{
    public static class AttachmentFileNameNormalizer
    {
        /// <summary>
        /// Normalizes attachment file name for safe file system usage while preserving the extension format.
        /// </summary>
        /// <param name="fileName">Original attachment file name.</param>
        /// <returns>Normalized file name.</returns>
        public static string Normalize(string fileName)
        {
            const string defaultFileName = "attachment";

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return defaultFileName;
            }

            var fileNameOnly = Path.GetFileName(fileName);
            var extension = NormalizeExtension(Path.GetExtension(fileNameOnly));
            var fileNameWithoutExtension = NormalizeFileNamePart(Path.GetFileNameWithoutExtension(fileNameOnly));

            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                fileNameWithoutExtension = defaultFileName;
            }

            return string.IsNullOrWhiteSpace(extension)
                ? fileNameWithoutExtension
                : fileNameWithoutExtension + extension;
        }

        private static string NormalizeExtension(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalizedExtension = NormalizeFileNamePart(value.TrimStart('.'));

            return string.IsNullOrWhiteSpace(normalizedExtension)
                ? string.Empty
                : "." + normalizedExtension;
        }

        private static string NormalizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var normalizedFileNameChars = value.ToCharArray();

            for (int i = 0; i < normalizedFileNameChars.Length; i++)
            {
                if (Array.IndexOf(invalidFileNameChars, normalizedFileNameChars[i]) >= 0)
                {
                    normalizedFileNameChars[i] = '_';
                }
            }

            return new string(normalizedFileNameChars).Trim(' ', '.');
        }
    }
}
