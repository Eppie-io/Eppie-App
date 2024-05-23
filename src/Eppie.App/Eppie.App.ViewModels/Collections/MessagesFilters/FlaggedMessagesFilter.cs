namespace Tuvi.App.ViewModels
{
    public class FlaggedMessagesFilter : MessageFilter
    {
        public FlaggedMessagesFilter()
        {
        }
        public FlaggedMessagesFilter(string label)
        {
            Label = label;
        }

        public override bool ItemPassedFilter(MessageInfo item)
        {
            return !(item == null || item.WasDeleted) && item.IsFlagged;
        }
    }
}
