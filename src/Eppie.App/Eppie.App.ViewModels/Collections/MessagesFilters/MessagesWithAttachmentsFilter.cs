namespace Tuvi.App.ViewModels
{
    public class MessagesWithAttachmentsFilter : MessageFilter
    {
        public MessagesWithAttachmentsFilter()
        {
        }
        public MessagesWithAttachmentsFilter(string label)
        {
            Label = label;
        }

        public override bool ItemPassedFilter(MessageInfo item)
        {
            return !(item == null || item.WasDeleted) && item.HasAttachments;
        }
    }
}
