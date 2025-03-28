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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels
{
    /// <summary>
    /// This interface represents a data source whose items can be loaded incrementally.
    /// </summary>
    /// <typeparam name="TSource">Type of collection element.</typeparam>
    public interface IIncrementalSource<TSource>
    {
        /// <summary>
        /// This method is invoked everytime the view need to show more items.
        /// </summary>
        /// <param name="count">The number of <typeparamref name="TSource"/> items to retrieve</param>
        /// <param name="cancellationToken">Used to propagate notification that operation should be canceled.</param>
        /// <returns></returns>
        Task<IEnumerable<TSource>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken);

        /// <summary>
        /// Reset source. Makes source to returm items from the begining
        /// </summary>
        void Reset();
    }
}
