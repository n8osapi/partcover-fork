using System;
using PartCover.Framework.Walkers;

namespace PartCover.Browser.Api
{
    public interface ICoverageReportService
    {
        event EventHandler<EventArgs> ReportClosing;
        event EventHandler<EventArgs> ReportOpened;

        ICoverageReport Report { get;}
        string ReportFileName { get;}

        void LoadFromFile(string fileName);

        void SaveReport(string fileName);

        void Load(CoverageReport report);
    }
}
