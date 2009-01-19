using System;
using PartCover.Browser.Helpers;
using PartCover.Browser.Api;
using System.Windows.Forms;

namespace PartCover.Browser.Dialogs
{
    internal partial class SmallAsyncUserForm : AsyncUserProcessForm, IProgressTracker
    {
        public SmallAsyncUserForm()
        {
            InitializeComponent();
        }

        private delegate void StringSetter(string value);

        public void PutDate() {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(PutDate));
                return;
            }

            tbText.AppendText(DateTime.Now.ToString() + " ");
        }

        public void PutText(string message) {
            if (InvokeRequired)
            {
                Invoke(new StringSetter(PutText), message);
                return;
            }

            tbText.AppendText(message);
        }

        public void SetMessage(string value)
        {
            PutDate();
            PutText(value + Environment.NewLine);
        }

        public float Percent
        {
            get { return 0; }
            set { }
        }

        public void QueueBegin(string message)
        {
            PutDate();
            PutText(message);
        }

        public void QueuePush(string message)
        {
            PutText(message);
        }

        public void QueueEnd(string message)
        {
            PutText(message + Environment.NewLine);
        }
    }
}