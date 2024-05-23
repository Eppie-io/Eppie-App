namespace Tuvi.App.ViewModels
{
    public class UnreadMessagesFilter : MessageFilter
    {
        public UnreadMessagesFilter()
        {
        }
        public UnreadMessagesFilter(string label)
        {
            Label = label;
        }

        public override bool ItemPassedFilter(MessageInfo item)
        {
            return !(item == null || item.WasDeleted) && !item.IsMarkedAsRead;
        }
    }
}
