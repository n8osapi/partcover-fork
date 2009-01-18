using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PartCover.MSBuild
{
    /// <summary>
    /// Execute the PartCover coverage.
    /// </summary>
    /// <remarks>
    /// <![CDATA[
    ///  <PartCover ToolPath="$(LibDirectory)\PartCover\"
    ///			    Target="$(LibDirectory)\NUnit\nunit-console.exe"
    ///			    TargetArgs="%(TestAssemblies.FullPath) /xml=%(TestAssemblies.Filename).xml /labels /nologo /noshadow"
    ///             WorkingDirectory="$(MSBuildProjectDirectory)"
    ///			    Output="partcover.xml"
    ///             Include="[AssemblyName.*]*"
    ///             Exclude="[*.Test]*"
    ///	/>
    /// ]]>
    /// </remarks>
    public class PartCover : ToolTask
    {
        private const string DefaultApplicationName = "PartCover.exe";
        private const string CorDriverName = "PartCover.CorDriver.dll";
        private string _targetArgs;
        private ITaskItem[] _exclude;
        private ITaskItem[] _include;
        private string _output;
        private string _format;
        private string _target;
        private string _workingDirectory;

        protected override string ToolName
        {
            get { return DefaultApplicationName; }
        }

        /// <summary>
        /// The application to execute to get the coverage results.
        /// Generally this will be your unit testing exe.
        /// </summary>
        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }

        /// <summary>
        /// The arguments to pass to the <see cref="Target"/> executable
        /// </summary>
        public string TargetArgs
        {
            get { return _targetArgs; }
            set { _targetArgs = value; }
        }

        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set { _workingDirectory = value; }
        }

        /// <summary>
        /// The file where partcover will write its output.
        /// </summary>
        public string Output
        {
            get { return _output; }
            set { _output = value; }
        }

        public string ReportFormat
        {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// The assembly expressions to include in the coverage.
        /// </summary>
        /// <example>
        /// [AssemblyName.*]*
        /// </example>
        public ITaskItem[] Include
        {
            get { return _include; }
            set { _include = value; }
        }

        /// <summary>
        /// The assembly expressions to exclude from the coverage.
        /// </summary>
        /// <example>
        /// Exclude="[*.Test]*"
        /// </example>
        public ITaskItem[] Exclude
        {
            get { return _exclude; }
            set { _exclude = value; }
        }

        public override bool Execute()
        {
            string corDriverPath = Path.Combine(ToolPath, CorDriverName);
            Log.LogMessage("CoreDriver: {0}", corDriverPath);
            using (Registrar registrar = new Registrar(corDriverPath))
            {
                registrar.RegisterComDLL();
                return base.Execute();
            }
        }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolName);
        }

        protected override string GenerateCommandLineCommands()
        {
            StringBuilder builder = new StringBuilder();
            AppendIfPresent(builder, "--target", Target);
            AppendIfPresent(builder, "--target-work-dir", WorkingDirectory);
            AppendIfPresent(builder, "--target-args", QuoteIfNeeded(TargetArgs));
            AppendIfPresent(builder, "--output", Output);
            AppendIfPresent(builder, "--reportFormat", ReportFormat);

            AppendMultipleItemsTo(builder, "--include", Include);
            AppendMultipleItemsTo(builder, "--exclude", Exclude);

            Log.LogCommandLine(builder.ToString());

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(String singleLine, MessageImportance messageImportance)
        {
            // This gets it to ouput the information coming from the subtask, e.g. the NUnit output
            base.LogEventsFromTextOutput(singleLine, MessageImportance.High);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// Argument specifies arguments for target process. If target argument contains spaces 
        /// - quote <argument>. If you want specify quote (") in <arguments>, then precede it 
        /// by slash (\)
        /// </remarks>
        private static string QuoteIfNeeded(string args)
        {
            if (String.IsNullOrEmpty(args))
                return args;
            if (! args.Contains(" "))
                return args;

            // Escape internal quotes if any
            if (args.Contains("\"")) {
                args = args.Replace("\"", "\\\"");
            }

            // quote string
            return String.Format("\"{0}\"", args);
        }

        private static void AppendIfPresent(StringBuilder builder, string cmdArg, string value)
        {
            if (! String.IsNullOrEmpty(value))
                builder.AppendFormat("{0} {1} ", cmdArg, value);
        }

        private static void AppendMultipleItemsTo(StringBuilder builder, string cmdArg, IEnumerable<ITaskItem> items)
        {
            if (null == items) return;

            foreach (ITaskItem item in items)
            {
                builder.AppendFormat("{0} {1} ", cmdArg, item.ItemSpec);
            }
        }
    }
}
