using System.Collections.Generic;

namespace Tuvi.App.ViewModels
{
    public interface IExtendedComparer<T> : IComparer<T>, IEqualityComparer<T>
    {
    }
}
