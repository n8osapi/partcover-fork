using System;
using System.Windows.Forms;
using PartCover.Browser.Stuff;
using PartCover.Browser.Helpers;
using PartCover.Framework.Walkers;
using System.Globalization;

namespace PartCover.Browser.Forms
{
    internal partial class RunTargetTracker : AsyncUserProcessForm, IRunTargetProgressTracker
    {
        DateTime startedAt;

        public RunTargetTracker()
        {
            InitializeComponent();
            tssMessage.Text = string.Empty;
            tssTime.Text = string.Empty;
        }

        private void PutText(string message, bool clearBeforePut)
        {
            if (clearBeforePut)
            {
                tssMessage.Text = message;
            }
            else
            {
                tssMessage.Text = tssMessage.Text + message;
            }
        }

        private delegate void PutStringDelegate(string message);
        public void SetMessage(string message)
        {
            if (!IsHandleCreated) return;

            if (InvokeRequired)
            {
                Invoke(new PutStringDelegate(SetMessage), message);
                return;
            }

            PutText(message, true);
        }

        public void QueueBegin(string message)
        {
            if (!IsHandleCreated) return;

            if (InvokeRequired)
            {
                Invoke(new PutStringDelegate(QueueBegin), message);
                return;
            }

            PutText(message, true);
        }

        public void QueuePush(string message)
        {
            if (!IsHandleCreated) return;

            if (InvokeRequired)
            {
                Invoke(new PutStringDelegate(QueuePush), message);
                return;
            }
            PutText(message, false);
        }

        public void QueueEnd(string message)
        {
            if (!IsHandleCreated) return;

            if (InvokeRequired)
            {
                Invoke(new PutStringDelegate(QueueEnd), message);
                return;
            }
            PutText(message, false);
        }

        public float Percent
        {
            get { return 0; }
            set { }
        }

        protected override void BeforeStart()
        {
            startedAt = DateTime.Now;
            timer.Enabled = true;
        }

        public void Add(CoverageReport.RunLogMessage runLogMessage)
        {
            PutLogEntry(runLogMessage);
        }

        delegate void PutLogEntryDelegate(CoverageReport.RunLogMessage item);
        private void PutLogEntry(CoverageReport.RunLogMessage item)
        {
            if (InvokeRequired)
            {
                Invoke(new PutLogEntryDelegate(PutLogEntry), item);
                return;
            }

            tbLog.AppendText(string.Format(CultureInfo.CurrentCulture,
                "[{0,6}][{1,6}]{2}{3}",
                item.ThreadId, item.MsOffset, item.Message, Environment.NewLine));

        }

        public void Add(CoverageReport.RunHistoryMessage runHistoryMessage)
        {
            SetMessage(runHistoryMessage.Message);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!IsHandleCreated) return;

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(PutTime));

            }
            else
            {
                PutTime();
            }
        }

        private void PutTime()
        {
            TimeSpan span = DateTime.Now - startedAt;

            string messsage = string.Empty;
            if (span.Minutes > 0)
            {
                messsage += string.Format("{0,2} min ", span.Minutes);
            }
            if (span.Seconds > 0)
            {
                messsage += string.Format("{0,2} sec", span.Seconds);
            }

            if (tssTime.Text != messsage)
            {
                tssTime.Text = messsage;
            }
        }
    }
}