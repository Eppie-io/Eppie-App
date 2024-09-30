namespace Tuvi.App.ViewModels
{
    public abstract class ContactComparer : IExtendedComparer<ContactItem>
    {
        public string Label { get; set; } = "";

        public abstract int Compare(ContactItem x, ContactItem y);

        public bool Equals(ContactItem x, ContactItem y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(ContactItem obj)
        {
            return obj?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}

