using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class SearchMessageFilter : ObservableObject, ISearchFilter<MessageInfo>
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private static bool EmailContains(EmailAddress email, string text)
        {
            return StringHelper.StringContainsIgnoreCase(email.Address, text)
                || StringHelper.StringContainsIgnoreCase(email.Name, text);
        }

        public bool ItemPassedFilter(MessageInfo item)
        {
            if (item == null || item.WasDeleted)
            {
                return false;
            }

            var message = item.MessageData;

            return string.IsNullOrEmpty(SearchText)
                || StringHelper.StringContainsIgnoreCase(message.Subject, SearchText)
                || StringHelper.StringContainsIgnoreCase(message.PreviewText, SearchText)
                || StringHelper.StringContainsIgnoreCase(message.TextBody, SearchText)
                || message.From.Exists(x => EmailContains(x, SearchText))
                || message.To.Exists(x => EmailContains(x, SearchText))
                || message.ReplyTo.Exists(x => EmailContains(x, SearchText))
                || message.Cc.Exists(x => EmailContains(x, SearchText))
                || message.Bcc.Exists(x => EmailContains(x, SearchText))
            ;
        }
    }
}
