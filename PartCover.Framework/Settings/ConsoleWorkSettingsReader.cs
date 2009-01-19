using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace PartCover.Framework.Settings
{
    public class ConsoleWorkSettingsReader
    {
        private class ArgumentOption
        {
            public delegate void ActivateHandler(WorkSettings settings);
            public delegate void OptionHandler(WorkSettings settings, string value);

            public readonly ActivateHandler activator;
            public readonly OptionHandler handler;
            public readonly string key;
            public string arguments;
            public string description;
            public bool optional;

            public ArgumentOption(string key, OptionHandler handler)
                : this(key, handler, null, true, string.Empty, string.Empty)
            {
            }

            public ArgumentOption(string key, ActivateHandler activator)
                : this(key, null, activator, true, string.Empty, string.Empty)
            {
            }

            public ArgumentOption(string key, OptionHandler handler, ActivateHandler activator, bool optional,
                                  string arguments, string description)
            {
                this.key = key;
                this.handler = handler;
                this.activator = activator;
                this.optional = optional;
                this.arguments = arguments;
                this.description = description;
            }
        }

        private static readonly ArgumentOption[] Options =
            {
                new ArgumentOption("--target", ReadTarget),
                new ArgumentOption("--version", ReadVersion),
                new ArgumentOption("--help", ReadHelp),
                new ArgumentOption("--target-work-dir", ReadTargetWorkDir),
                new ArgumentOption("--generate", ReadGenerateSettingsFile),
                new ArgumentOption("--log", ReadLogLevel),
                new ArgumentOption("--target-args", ReadTargetArgs),
                new ArgumentOption("--include", ReadInclude),
                new ArgumentOption("--exclude", ReadExclude),
                new ArgumentOption("--output", ReadOutput),
                new ArgumentOption("--reportFormat", ReadReportFormat),
                new ArgumentOption("--settings", ReadSettingsFile),
            };

        private ArgumentOption currentOption;

        #region settings readers

        private static void ReadTarget(WorkSettings settings, string value)
        {
            if (!File.Exists(value))
                throw new SettingsException("Cannot find target (" + value + ")");
            settings.TargetPath = Path.GetFullPath(value);
        }

        private static void ReadGenerateSettingsFile(WorkSettings settings, string value)
        {
            settings.GenerateSettingsFileName = value;
        }

        private static void ReadHelp(WorkSettings settings)
        {
            settings.PrintLongHelp = true;
        }

        private static void ReadVersion(WorkSettings settings)
        {
            settings.PrintVersion = true;
        }

        private static void ReadTargetWorkDir(WorkSettings settings, string value)
        {
            if (!Directory.Exists(value))
                throw new SettingsException("Cannot find target working dir (" + value + ")");
            settings.TargetWorkingDir = Path.GetFullPath(value);
        }

        private static void ReadSettingsFile(WorkSettings settings, string value)
        {
            if (!File.Exists(value))
                throw new SettingsException("Cannot find settings file (" + value + ")");
            settings.SettingsFile = value;
        }

        private static void ReadOutput(WorkSettings settings, string value)
        {
            if (value.Length > 0) settings.FileNameForReport = value;
        }

        private static void ReadReportFormat(WorkSettings settings, string value)
        {
            if (value.Length > 0) settings.ReportFormat = value;
        }

        private static void ReadExclude(WorkSettings settings, string value)
        {
            if (value.Length > 0) settings.ExcludeRules(value);
        }

        private static void ReadInclude(WorkSettings settings, string value)
        {
            if (value.Length > 0) settings.IncludeRules(value);
        }

        private static void ReadTargetArgs(WorkSettings settings, string value)
        {
            settings.TargetArgs = value;
        }

        private static void ReadLogLevel(WorkSettings settings, string value)
        {
            try
            {
                settings.LogLevel = Int32.Parse(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new SettingsException("Wrong value for --log (" + ex.Message + ")");
            }
        }

        #endregion

        private readonly WorkSettingsReader settingsReader;
        private readonly WorkSettingsWriter settingsWriter;

        public ConsoleWorkSettingsReader(WorkSettingsReader reader, WorkSettingsWriter writer)
        {
            settingsReader = reader;
            settingsWriter = writer;
        }

        public WorkSettings InitializeFromCommandLine(string[] args)
        {
            WorkSettings settings = new WorkSettings();
            currentOption = null;

            foreach (string arg in args)
            {
                ArgumentOption nextOption = GetOptionHandler(arg);
                if (nextOption != null)
                {
                    currentOption = nextOption;

                    if (currentOption.activator != null)
                        currentOption.activator(settings);

                    continue;
                }

                if (currentOption == null)
                {
                    PrintShortUsage(true);
                    return null;
                }

                if (currentOption.handler != null)
                {
                    currentOption.handler(settings, arg);
                }
                else
                {
                    throw new SettingsException("Unexpected argument for option '" + currentOption.key + "'");
                }
            }

            if (settings.SettingsFile != null)
            {
                settings = settingsReader.ReadSettingsFile(settings.SettingsFile);
            }
            else if (settings.GenerateSettingsFileName != null)
            {
                settingsWriter.GenerateSettingsFile(settings);
                return null;
            }
            bool showShort = true;
            if (settings.PrintLongHelp)
            {
                showShort = false;
                PrintVersionInfo();
                PrintShortUsage(false);
                PrintLongUsage();
            }
            else if (settings.PrintVersion)
            {
                PrintVersionInfo();
            }

            if (!String.IsNullOrEmpty(settings.TargetPath))
                return settings;

            if (showShort)
            {
                PrintShortUsage(true);
            }
            return null;
        }

        private static ArgumentOption GetOptionHandler(string arg)
        {
            foreach (ArgumentOption o in Options)
            {
                if (o.key.Equals(arg, StringComparison.CurrentCulture))
                    return o;
            }

            if (arg.StartsWith("--"))
            {
                throw new SettingsException("Invalid option '" + arg + "'");
            }

            return null;
        }

        public static void PrintShortUsage(bool showNext)
        {
            Console.Out.WriteLine("Usage:");
            Console.Out.WriteLine("  PartCover.exe  --target <file_name> [--target-work-dir <path>]");
            Console.Out.WriteLine("                [--target-args <arguments>] [--settings <file_name>]");
            Console.Out.WriteLine("                [--include <item> ... ] [--exclude <item> ... ]");
            Console.Out.WriteLine("                [--output <file_name>] [--log <log_level>]");
            Console.Out.WriteLine("                [--generate <file_name>] [--help] [--version]");
            Console.Out.WriteLine("");
            if (showNext)
            {
                Console.Out.WriteLine("For more help execute:");
                Console.Out.WriteLine("  PartCover.exe --help");
            }
        }

        public static void PrintLongUsage()
        {
            Console.Out.WriteLine("Arguments:  ");
            Console.Out.WriteLine("   --target=<file_name> :");
            Console.Out.WriteLine("       specifies path to executable file to count coverage. <file_name> may be");
            Console.Out.WriteLine("       either full path or relative path to file.");
            Console.Out.WriteLine("   --target-work-dir=<path> :");
            Console.Out.WriteLine("       specifies working directory to target process. By default, working");
            Console.Out.WriteLine("       directory will be working directory for PartCover");
            Console.Out.WriteLine("   --target-args=<arguments> :");
            Console.Out.WriteLine("       specifies arguments for target process. If target argument contains");
            Console.Out.WriteLine("       spaces - quote <argument>. If you want specify quote (\") in <arguments>,");
            Console.Out.WriteLine("       then precede it by slash (\\)");
            Console.Out.WriteLine("   --include=<item>, --exclude=<item> :");
            Console.Out.WriteLine(
                "       specifies item to include or exclude from report. Item is in following format: ");
            Console.Out.WriteLine("          [<assembly_regexp>]<class_regexp>");
            Console.Out.WriteLine("       where <regexp> is simple regular expression, containing only asterix and");
            Console.Out.WriteLine("       characters to point item. For example:");
            Console.Out.WriteLine("          [mscorlib]*");
            Console.Out.WriteLine("          [System.*]System.IO.*");
            Console.Out.WriteLine("          [System]System.Colle*");
            Console.Out.WriteLine("          [Test]Test.*+InnerClass+SecondInners*");
            Console.Out.WriteLine("   --settings=<file_name> :");
            Console.Out.WriteLine("       specifies input settins in xml file.");
            Console.Out.WriteLine("   --generate=<file_name> :");
            Console.Out.WriteLine("       generates setting file using settings specified. By default, <file_name>");
            Console.Out.WriteLine("       is 'PartCover.settings.xml'");
            Console.Out.WriteLine("   --output=<file_name> :");
            Console.Out.WriteLine("       specifies output file for writing result xml. It will be placed in UTF-8");
            Console.Out.WriteLine("       encoding. By default, output data will be processed via console output.");
            Console.Out.WriteLine("   --log=<log_level> :");
            Console.Out.WriteLine("       specifies log level for driver. If <log_level> greater than 0, log file");
            Console.Out.WriteLine("       will be created in working directory for PartCover");
            Console.Out.WriteLine("   --help :");
            Console.Out.WriteLine("       shows current help");
            Console.Out.WriteLine("   --version :");
            Console.Out.WriteLine("       shows version of PartCover console application");
            Console.Out.WriteLine("");
        }

        public static void PrintVersionInfo()
        {
            Console.Out.WriteLine("PartCover (console)");
            Console.Out.WriteLine("   application version {0}.{1}.{2}",
                                  Assembly.GetExecutingAssembly().GetName().Version.Major,
                                  Assembly.GetExecutingAssembly().GetName().Version.Minor,
                                  Assembly.GetExecutingAssembly().GetName().Version.Revision);
            Type connector = typeof (Connector);
            Console.Out.WriteLine("   connector version {0}.{1}.{2}",
                                  connector.Assembly.GetName().Version.Major,
                                  connector.Assembly.GetName().Version.Minor,
                                  connector.Assembly.GetName().Version.Revision);
            Console.Out.WriteLine("");
        }
    }
}
