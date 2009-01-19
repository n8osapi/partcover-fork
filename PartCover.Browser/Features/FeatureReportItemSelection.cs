using System;

using PartCover.Browser.Api;

namespace PartCover.Browser.Features
{
    public class FeatureReportItemSelection
        : IFeature
        , IReportItemSelectionService
    {
        public void Attach(IServiceContainer container) { }

        public void Detach(IServiceContainer container) { }

        public void Build(IServiceContainer container)
        {
            container.GetService<ICoverageReportService>().ReportClosing += onReportClosing;
            container.GetService<ICoverageReportService>().ReportOpened += onReportOpened;
        }

        public void Destroy(IServiceContainer container)
        {
            container.GetService<ICoverageReportService>().ReportClosing -= onReportClosing;
            container.GetService<ICoverageReportService>().ReportOpened -= onReportOpened;
        }

        private IReportItem selectedItem;
        public IReportItem SelectedItem
        {
            get { return selectedItem; }
        }

        void onReportOpened(object sender, EventArgs e) { }

        void onReportClosing(object sender, EventArgs e) { }

        public void select<T>(T item) where T : IReportItem
        {
            selectedItem = item;
            fireSelectionChanged();
        }

        public void selectNone()
        {
            selectedItem = null;
            fireSelectionChanged();
        }

        private void fireSelectionChanged()
        {
            if (SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
        }

        public event EventHandler SelectionChanged;
    }
}
