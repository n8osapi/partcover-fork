using System;
using System.Collections.Generic;

using PartCover.Browser.Api.ReportItems;
using PartCover.Framework.Walkers;

namespace PartCover.Browser.Api
{
    public interface ICoverageReport
    {
        IAssembly[] GetAssemblies();

        string GetFilePath(uint file);

        void ForEachBlock(Action<CoverageReport.InnerBlock> blockReceiver);

        ICollection<CoverageReport.RunHistoryMessage> GetRunHistory();

        ICollection<CoverageReport.RunLogMessage> GetLogEvents();

        int? GetExitCode();
    }
}
