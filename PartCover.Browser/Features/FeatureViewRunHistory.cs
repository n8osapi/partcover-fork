using PartCover.Browser.Api;
using PartCover.Browser.Features.Views;

namespace PartCover.Browser.Features
{
    public class FeatureViewRunHistory
        : IFeature
        , IReportViewFactory
    {
        public void Build(IServiceContainer container)
        {
            container.GetService<IReportViewValve>().Add(this);
        }

        public void Destroy(IServiceContainer container)
        {
            container.GetService<IReportViewValve>().Remove(this);
        }

        public void Attach(IServiceContainer container) { }

        public void Detach(IServiceContainer container) { }

        public ReportView create()
        {
            return new RunHistoryView();
        }

        public string ViewName
        {
            get { return "Run History"; }
        }
    }
}
