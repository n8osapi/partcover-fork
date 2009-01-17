using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PartCover.Framework.Walkers
{
    public class DefaultReportWriter : IReportWriter
    {
        public virtual void WriteReport(CoverageReport report, TextWriter writer)
        {
            XmlDocument xml = new XmlDocument();

            XmlNode root = xml.AppendChild(xml.CreateElement("PartCoverReport"));
            root.Attributes.Append(xml.CreateAttribute("ver")).Value = VersionString(GetHelperAssembly().GetName().Version);

            if (report.ExitCode.HasValue)
                root.Attributes.Append(xml.CreateAttribute("exitCode")).Value = report.ExitCode.Value.ToString(CultureInfo.InvariantCulture);

            foreach (CoverageReport.FileDescriptor dFile in report.files)
            {
                XmlNode fileNode = root.AppendChild(xml.CreateElement("file"));
                fileNode.Attributes.Append(xml.CreateAttribute("id")).Value = dFile.fileId.ToString(CultureInfo.InvariantCulture);
                fileNode.Attributes.Append(xml.CreateAttribute("url")).Value = dFile.fileUrl;
            }
            foreach (CoverageReport.AssemblyDescriptor assembly in report.assemblies)
            {
                foreach (CoverageReport.TypeDescriptor dType in assembly.types)
                {
                    XmlNode typeNode = root.AppendChild(xml.CreateElement("type"));
                    typeNode.Attributes.Append(xml.CreateAttribute("asm")).Value = dType.assemblyName;
                    typeNode.Attributes.Append(xml.CreateAttribute("name")).Value = dType.typeName;
                    typeNode.Attributes.Append(xml.CreateAttribute("flags")).Value =
                        dType.flags.ToString(CultureInfo.InvariantCulture);

                    foreach (CoverageReport.MethodDescriptor dMethod in dType.methods)
                    {
                        XmlNode methodNode = typeNode.AppendChild(xml.CreateElement("method"));
                        methodNode.Attributes.Append(xml.CreateAttribute("name")).Value = dMethod.methodName;
                        methodNode.Attributes.Append(xml.CreateAttribute("sig")).Value = dMethod.methodSig;
                        methodNode.Attributes.Append(xml.CreateAttribute("flags")).Value =
                            dMethod.flags.ToString(CultureInfo.InvariantCulture);
                        methodNode.Attributes.Append(xml.CreateAttribute("iflags")).Value =
                            dMethod.implFlags.ToString(CultureInfo.InvariantCulture);

                        foreach (CoverageReport.InnerBlockData bData in dMethod.insBlocks)
                        {
                            XmlNode codeNode = methodNode.AppendChild(xml.CreateElement("code"));
                            foreach (CoverageReport.InnerBlock inner in bData.blocks)
                            {
                                XmlNode point = codeNode.AppendChild(xml.CreateElement("pt"));
                                point.Attributes.Append(xml.CreateAttribute("visit")).Value =
                                    inner.visitCount.ToString(CultureInfo.InvariantCulture);
                                point.Attributes.Append(xml.CreateAttribute("pos")).Value =
                                    inner.position.ToString(CultureInfo.InvariantCulture);
                                point.Attributes.Append(xml.CreateAttribute("len")).Value =
                                    inner.blockLen.ToString(CultureInfo.InvariantCulture);
                                if (inner.fileId != 0)
                                {
                                    point.Attributes.Append(xml.CreateAttribute("fid")).Value =
                                        inner.fileId.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("sl")).Value =
                                        inner.startLine.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("sc")).Value =
                                        inner.startColumn.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("el")).Value =
                                        inner.endLine.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("ec")).Value =
                                        inner.endColumn.ToString(CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                }
            }

            XmlNode runNode = root.AppendChild(xml.CreateElement("run"));

            XmlNode runTrackerNode = runNode.AppendChild(xml.CreateElement("tracker"));
            foreach (CoverageReport.RunHistoryMessage rhm in report.runHistory)
            {
                XmlNode messageNode = runTrackerNode.AppendChild(xml.CreateElement("message"));
                messageNode.Attributes.Append(xml.CreateAttribute("tm")).Value = rhm.Time.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture);
                messageNode.InnerText = rhm.Message;
            }

            XmlNode runLogNode = runNode.AppendChild(xml.CreateElement("log"));
            foreach (CoverageReport.RunLogMessage rlm in report.runLog)
            {
                XmlNode messageNode = runLogNode.AppendChild(xml.CreateElement("message"));
                messageNode.Attributes.Append(xml.CreateAttribute("tr")).Value = rlm.ThreadId.ToString(CultureInfo.InvariantCulture);
                messageNode.Attributes.Append(xml.CreateAttribute("ms")).Value = rlm.MsOffset.ToString(CultureInfo.InvariantCulture);
                messageNode.InnerText = rlm.Message;
            }

            xml.Save(writer);
        }

        public string VersionString(Version version)
        {
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public virtual Assembly GetHelperAssembly()
        {
            return Assembly.GetAssembly(typeof(DefaultReportWriter));
        }
    }
}