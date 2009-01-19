using PartCover.Framework.Walkers;

namespace PartCover.Browser.Api.ReportItems
{
    public interface ICoveredVariant : IReportItem
    {
        CoverageReport.InnerBlock[] Blocks { get; }
    }
}
