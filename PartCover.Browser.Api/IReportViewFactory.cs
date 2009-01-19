
namespace PartCover.Browser.Api
{
    public interface IReportViewFactory
    {
        ReportView create();

        string ViewName { get; }
    }
}
