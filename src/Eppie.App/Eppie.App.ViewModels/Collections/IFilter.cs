namespace Tuvi.App.ViewModels
{
    public interface IFilter<TSource>
    {
        bool ItemPassedFilter(TSource item);
    }
}
