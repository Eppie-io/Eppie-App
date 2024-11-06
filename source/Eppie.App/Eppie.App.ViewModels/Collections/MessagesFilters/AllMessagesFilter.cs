namespace Tuvi.App.ViewModels
{
    public class AllMessagesFilter : MessageFilter
    {
        public AllMessagesFilter()
        {
        }
        public AllMessagesFilter(string label)
        {
            Label = label;
        }

        public override bool ItemPassedFilter(MessageInfo item)
        {
            return !(item == null || item.WasDeleted);
        }
    }
}
