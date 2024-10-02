using System.ComponentModel;

namespace Tuvi.App.ViewModels
{
    public interface ISearchFilter<TSource> : IFilter<TSource>, INotifyPropertyChanged
    {
        string SearchText { get; set; }
    }
}
