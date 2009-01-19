using System.Collections.Generic;

namespace PartCover.Framework.Settings
{
    public class WorkSettings
    {

        private readonly List<string> excludeItems = new List<string>();
        private readonly List<string> includeItems = new List<string>();
        private string generateSettingsFileName;
        private int logLevel;
        private string outputFile;
        private string reportFormat;
        private string settingsFile;
        private string targetArgs;
        private string targetPath;
        private string targetWorkingDir;
        private bool printLongHelp;
        private bool printVersion;

        public string SettingsFile
        {
            get { return settingsFile; }
            set { settingsFile = value; }
        }

        public string GenerateSettingsFileName
        {
            get { return generateSettingsFileName; }
            set { generateSettingsFileName = value; }
        }

        public int LogLevel
        {
            get { return logLevel; }
            set { logLevel = value; }
        }

        public string[] IncludeItems
        {
            get { return includeItems.ToArray(); }
        }

        public string[] ExcludeItems
        {
            get { return excludeItems.ToArray(); }
        }

        public string TargetPath
        {
            get { return targetPath; }
            set { targetPath = value; }
        }

        public string TargetWorkingDir
        {
            get { return targetWorkingDir; }
            set { targetWorkingDir = value; }
        }

        public string TargetArgs
        {
            get { return targetArgs; }
            set { targetArgs = value; }
        }

        public string FileNameForReport
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        public string ReportFormat
        {
            get { return reportFormat; }
            set { reportFormat = value; }
        }

        public bool OutputToFile
        {
            get { return FileNameForReport != null; }
        }

        public bool PrintLongHelp
        {
            get { return printLongHelp; }
            set { printLongHelp = value; }
        }

        public bool PrintVersion
        {
            get { return printVersion; }
            set { printVersion = value; }
        }

        public void IncludeRules(ICollection<string> strings)
        {
            includeItems.AddRange(strings);
        }

        public void IncludeRules(string rule)
        {
            includeItems.Add(rule);
        }

        public void ExcludeRules(ICollection<string> strings)
        {
            excludeItems.AddRange(strings);
        }

        public void ExcludeRules(string rule)
        {
            excludeItems.Add(rule);
        }

        public void ClearRules()
        {
            includeItems.Clear();
            excludeItems.Clear();
        }
    }
}