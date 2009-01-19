using System;
using PartCover.Browser.Api;
using PartCover.Browser.Forms;
using PartCover.Browser.Helpers;
using PartCover.Framework;
using PartCover.Framework.Walkers;

namespace PartCover.Browser.Stuff
{
    internal interface IRunTargetProgressTracker : IProgressTracker
    {
        void Add(CoverageReport.RunLogMessage runLogMessage);
        void Add(CoverageReport.RunHistoryMessage runHistoryMessage);
    }

    internal class TargetRunner : AsyncUserProcess<RunTargetTracker, IRunTargetProgressTracker>
    {
        private CoverageReport report;
        private RunTargetForm runTargetForm;

        public RunTargetForm RunTargetForm
        {
            get { return runTargetForm; }
            set { runTargetForm = value; }
        }

        public CoverageReport Report
        {
            get { return report; }
        }

        protected override void doWork()
        {
            Connector connector = new Connector();
            connector.ProcessCallback.OnMessage += ConnectorOnMessage;
            connector.OnEventMessage += ConnectorOnEventMessage;

            Tracker.SetMessage("Create connector");

            if (runTargetForm.InvokeRequired)
            {
                runTargetForm.Invoke(new InitializeConnectorDelegate(InitializeConnector), connector);
            }
            else
            {
                InitializeConnector(connector);
            }


            Tracker.SetMessage("Store report");
            report = connector.BlockWalker.Report;

            Tracker.SetMessage("Done");
            connector.OnEventMessage -= ConnectorOnEventMessage;
            connector.ProcessCallback.OnMessage -= ConnectorOnMessage;
        }

        private void InitializeConnector(Connector connector)
        {
            foreach (string s in runTargetForm.IncludeItems) connector.IncludeItem(s);
            foreach (string s in runTargetForm.ExcludeItems) connector.ExcludeItem(s);

            connector.SetLogging(runTargetForm.LogLevel);
            connector.UseFileLogging(false);
            connector.UsePipeLogging(true);
            connector.StartTarget(
                runTargetForm.TargetPath,
                runTargetForm.TargetWorkingDir,
                runTargetForm.TargetArgs,
                false, false);
        }

        private event EventHandler<EventArgs<CoverageReport.RunHistoryMessage>> OnMessage;
        private event EventHandler<EventArgs<CoverageReport.RunLogMessage>> OnEventMessage;

        private void ConnectorOnEventMessage(object sender, EventArgs<CoverageReport.RunLogMessage> e)
        {
            if (OnEventMessage != null) OnEventMessage(this, e);
            Tracker.Add(e.Data);
        }

        private void ConnectorOnMessage(object sender, EventArgs<CoverageReport.RunHistoryMessage> e)
        {
            if (OnMessage != null) OnMessage(this, e);
            Tracker.Add(e.Data);
        }

        #region Nested type: InitializeConnectorDelegate

        private delegate void InitializeConnectorDelegate(Connector connector);

        #endregion
    }
}