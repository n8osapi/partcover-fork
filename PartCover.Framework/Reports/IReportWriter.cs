using System.IO;
using PartCover.Framework.Walkers;

namespace PartCover.Framework.Reports
{
    public interface IReportWriter
    {
        void WriteReport(CoverageReport report, TextWriter writer);
    }
}