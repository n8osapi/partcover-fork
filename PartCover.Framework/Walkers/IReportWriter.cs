using System.IO;

namespace PartCover.Framework.Walkers
{
    public interface IReportWriter
    {
        void WriteReport(CoverageReport report, TextWriter writer);
    }
}