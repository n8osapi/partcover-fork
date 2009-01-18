using System;
using System.IO;
using PartCover.Framework;
using PartCover.Framework.Reports;
using PartCover.Framework.Walkers;

namespace PartCover
{
    internal class ApplicationEntry
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                WorkSettings settings = new WorkSettings();
                if (!settings.InitializeFromCommandLine(args))
                {
                    return -1;
                }

                Connector connector = new Connector();
                connector.OnEventMessage += connector_OnEventMessage;
                connector.ProcessCallback.OnMessage += ProcessCallback_OnMessage;

                connector.UseFileLogging(true);
                connector.UsePipeLogging(false);
                connector.SetLogging((Logging) settings.LogLevel);

                foreach (string item in settings.IncludeItems)
                {
                    try
                    {
                        connector.IncludeItem(item);
                    }
                    catch (ArgumentException)
                    {
                        LogError("Item '{0}' has wrong format", item);
                    }
                }

                foreach (string item in settings.ExcludeItems)
                {
                    try
                    {
                        connector.ExcludeItem(item);
                    }
                    catch (ArgumentException)
                    {
                        LogError("Item '{0}' has wrong format", item);
                    }
                }

                connector.StartTarget(
                    settings.TargetPath,
                    settings.TargetWorkingDir,
                    settings.TargetArgs,
                    true,
                    false);

                try
                {
                    IReportWriter reportWriter;
                    if (settings.ReportFormat == "ncover")
                    {
                        reportWriter = new NCoverReportWriter();
                    }
                    else
                    {
                        reportWriter = new DefaultReportWriter();
                    }

                    if (settings.OutputToFile)
                    {
                        using (StreamWriter writer = File.CreateText(settings.FileNameForReport))
                        {
                            reportWriter.WriteReport(connector.BlockWalker.Report, writer);
                        }
                    }
                    else
                    {
                        reportWriter.WriteReport(connector.BlockWalker.Report, Console.Out);
                    }
                }
                catch (Exception ex)
                {
                    LogError("Can't save report ({0})", ex.Message);
                }

                if (connector.TargetExitCode.HasValue)
                    return connector.TargetExitCode.Value;
            }
            catch (SettingsException ex)
            {
                LogError(ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogError(ex.StackTrace);
                return -1;
            }

            return 0;
        }

        private static void LogError(string format, params string[] args)
        {
            Console.Error.WriteLine(format, args);
        }

        private static void ProcessCallback_OnMessage(object sender, EventArgs<CoverageReport.RunHistoryMessage> e)
        {
        }

        private static void connector_OnEventMessage(object sender, EventArgs<CoverageReport.RunLogMessage> e)
        {
        }
    }
}