using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class SearchContactFilter : ObservableObject, ISearchFilter<ContactItem>
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool ItemPassedFilter(ContactItem item)
        {
            return string.IsNullOrEmpty(SearchText)
                || StringHelper.StringContains(item?.FullName, SearchText, StringComparison.CurrentCultureIgnoreCase)
                || StringHelper.EmailContains(item?.Email.Address, SearchText);
        }
    }
}
