
namespace PartCover.Browser.Api
{
    public interface IReportViewValve
    {
        void Add(IReportViewFactory factory);
        void Remove(IReportViewFactory factory);
    }
}
