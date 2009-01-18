using System;
using System.IO;
using PartCover.Framework.Walkers;

namespace PartCover.Framework
{
    public class Connector
    {
        private readonly ConnectorActionCallback actionWriter;
        private readonly PartCoverConnector2Class connector = new PartCoverConnector2Class();

        private readonly ProcessCallback processCallback;
        private InstrumentedBlocksWalkerInner blockWalker;

        public Connector()
        {
            actionWriter = new ConnectorActionCallback(this);
            processCallback = new ProcessCallback();
            processCallback.OnMessage += processCallbackOnMessage;
        }

        public ProcessCallback ProcessCallback
        {
            get { return processCallback; }
        }

        internal ConnectorActionCallback ActionCallback
        {
            get { return actionWriter; }
        }

        public InstrumentedBlocksWalker BlockWalker
        {
            get { return blockWalker; }
        }

        public int? TargetExitCode
        {
            get
            {
                if (connector != null && connector.HasTargetExitCode)
                    return connector.TargetExitCode;
                return null;
            }
        }

        public int TargetProcessId
        {
            get
            {
                if (connector == null)
                    throw new InvalidOperationException("No connector available");
                return connector.ProcessId;
            }
        }

        public string DriverLogFile
        {
            get
            {
                if (connector == null)
                    throw new InvalidOperationException("No connector available");
                return connector.LogFilePath;
            }
        }

        public event EventHandler<EventArgs<CoverageReport.RunLogMessage>> OnEventMessage;

        public void SetLogging(Logging logging)
        {
            connector.LoggingLevel = (int) logging;
        }

        public void UseFileLogging(bool logging)
        {
            connector.FileLoggingEnable = logging;
        }

        public void UsePipeLogging(bool logging)
        {
            connector.PipeLoggingEnable = logging;
        }

        public void StartTarget(string path, string directory, string args, bool redirectOutput, bool delayClose)
        {
            blockWalker = new InstrumentedBlocksWalkerInner();

            // set mode
            connector.EnableOption(ProfilerMode.COUNT_COVERAGE);

            ExcludeItem("[mscorlib]*");
            ExcludeItem("[System*]*");

            if (directory != null) directory = directory.Trim();
            if (path != null) path = path.Trim();
            if (args != null) args = args.Trim();

            if (directory == null || directory.Length == 0)
                directory = Directory.GetCurrentDirectory();

            // start target
            ProcessCallback.WriteStatus("Start target");
            connector.StartTarget(path, directory, args, redirectOutput, actionWriter);

            // wait results
            ProcessCallback.WriteStatus("Wait results");
            connector.WaitForResults(delayClose, ActionCallback);

            // walk results
            ProcessCallback.WriteStatus("Walk results");
            connector.WalkInstrumentedResults(blockWalker);

            if (connector.HasTargetExitCode) blockWalker.Report.ExitCode = connector.TargetExitCode;
        }

        internal void OnLogMessage(CoverageReport.RunLogMessage message)
        {
            blockWalker.Report.runLog.Add(message);
            if (OnEventMessage != null) OnEventMessage(this, new EventArgs<CoverageReport.RunLogMessage>(message));
        }

        private void processCallbackOnMessage(object sender, EventArgs<CoverageReport.RunHistoryMessage> e)
        {
            blockWalker.Report.runHistory.Add(e.Data);
        }

        public void CloseTarget()
        {
            connector.CloseTarget();
        }

        public void IncludeItem(string item)
        {
            connector.IncludeItem(item);
        }

        public void ExcludeItem(string item)
        {
            connector.ExcludeItem(item);
        }
    }
}