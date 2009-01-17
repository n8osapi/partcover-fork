using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PartCover.Framework.Walkers
{
    public class NCoverReportWriter : IReportWriter
    {
        public virtual void WriteReport(CoverageReport report, TextWriter writer)
        {
            XmlDocument xml = new XmlDocument();

            XmlNode root = xml.AppendChild(xml.CreateElement("coverage"));
            root.Attributes.Append(xml.CreateAttribute("profilerVersion")).Value = VersionString(GetHelperAssembly().GetName().Version);
            root.Attributes.Append(xml.CreateAttribute("driverVersion")).Value = VersionString(GetHelperAssembly().GetName().Version);

            // FIXME: Need start and end times
            root.Attributes.Append(xml.CreateAttribute("startTime")).Value = DateTime.Now.ToString();
            root.Attributes.Append(xml.CreateAttribute("measureTime")).Value = DateTime.Now.ToString();

            foreach (CoverageReport.AssemblyDescriptor assembly in report.assemblies)
            {
                XmlNode moduleNode = root.AppendChild(xml.CreateElement("module"));
                moduleNode.Attributes.Append(xml.CreateAttribute("name")).Value = assembly.assemblyPath;    // NCover calls the full path the 'name' and the assembly name the 'assembly'
                moduleNode.Attributes.Append(xml.CreateAttribute("assembly")).Value = assembly.name;
                moduleNode.Attributes.Append(xml.CreateAttribute("assemblyIdentity")).Value = assembly.assemblyIdentity;

                foreach (CoverageReport.TypeDescriptor dType in assembly.types)
                {
                    foreach (CoverageReport.MethodDescriptor dMethod in dType.methods)
                    {
                        XmlNode methodNode = moduleNode.AppendChild(xml.CreateElement("method"));
                        methodNode.Attributes.Append(xml.CreateAttribute("name")).Value = dMethod.methodName;
                        methodNode.Attributes.Append(xml.CreateAttribute("excluded")).Value = "false";
                        methodNode.Attributes.Append(xml.CreateAttribute("instruments")).Value = "true";
                        methodNode.Attributes.Append(xml.CreateAttribute("class")).Value = dType.typeName;

                        foreach (CoverageReport.InnerBlockData bData in dMethod.insBlocks)
                        {
                            foreach (CoverageReport.InnerBlock inner in bData.blocks)
                            {
                                XmlNode point = methodNode.AppendChild(xml.CreateElement("seqpnt"));
                                point.Attributes.Append(xml.CreateAttribute("visitcount")).Value = inner.visitCount.ToString(CultureInfo.InvariantCulture);
                                point.Attributes.Append(xml.CreateAttribute("excluded")).Value = "false";

                                if (inner.fileId != 0)
                                {
                                    point.Attributes.Append(xml.CreateAttribute("line")).Value = inner.startLine.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("column")).Value = inner.startColumn.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("endline")).Value = inner.endLine.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("endcolumn")).Value = inner.endLine.ToString(CultureInfo.InvariantCulture);
                                    point.Attributes.Append(xml.CreateAttribute("document")).Value = report.GetFileUrl(inner.fileId);
                                }
                            }
                        }
                    }
                }
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