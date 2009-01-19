using System;
using System.Windows.Forms;
using PartCover.Browser.Api;
using System.Reflection;
using log4net;

namespace PartCover.Browser
{
    public class MainEntry
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ApplicationHost host = new ApplicationHost();

            log.Info("search for features");
            foreach (IFeature feature in FeatureSeeker.Seek(Assembly.GetExecutingAssembly()))
            {
                log.Info("register feature: " + feature.GetType());
                host.RegisterService(feature);
            }

            host.Build();

            MainForm form = host.GetService<MainForm>();
            if (args.Length == 1)
            {
                host.GetService<ICoverageReportService>().LoadFromFile(args[0]);
            }

            Application.Run(form);

            host.Destroy();
        }
    }
}
