using PartCover.Browser.Api;

namespace PartCover.Browser.Features
{
    internal class BrowserFormFeature : IFeature
    {
        private MainForm mainForm;

        public MainForm MainForm
        {
            get { return mainForm; }
        }

        public void Attach(IServiceContainer container)
        {
            mainForm = new MainForm();
            mainForm.ServiceContainer = container;

            container.RegisterService(mainForm);
        }

        public void Detach(IServiceContainer container)
        {
            container.UnregisterService(mainForm);

            mainForm.ServiceContainer = null;
            mainForm = null;
        }

        public void Build(IServiceContainer container) { }

        public void Destroy(IServiceContainer container) { }
    }
}
