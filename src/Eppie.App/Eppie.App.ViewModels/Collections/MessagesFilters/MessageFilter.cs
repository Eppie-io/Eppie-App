namespace Tuvi.App.ViewModels
{
    public abstract class MessageFilter : IFilter<MessageInfo>
    {
        public string Label { get; set; } = "";

        public abstract bool ItemPassedFilter(MessageInfo item);

        public override string ToString()
        {
            return Label;
        }
    }
}
