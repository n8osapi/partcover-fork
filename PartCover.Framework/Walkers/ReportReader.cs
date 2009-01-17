using System;
using System.IO;
using System.Xml;

namespace PartCover.Framework.Walkers
{
    public class ReportReader
    {
        public void ReadReport(CoverageReport report, TextReader reader)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);

            XmlNode root = xml.SelectSingleNode("/PartCoverReport");
            if (root == null) throw new ReportException("Wrong report format");
            XmlAttribute verAttribute = root.Attributes["ver"];
            if (verAttribute == null) throw new ReportException("Wrong report format");
            XmlAttribute exitCodeAttribute = root.Attributes["exitCode"];
            if (exitCodeAttribute != null) report.ExitCode = GetIntAttribute(root, exitCodeAttribute.Name);

            foreach (XmlNode fileNode in xml.SelectNodes("/PartCoverReport/file"))
                report.AddFile(GetUInt32Attribute(fileNode, "id"), GetStringAttribute(fileNode, "url"));

            foreach (XmlNode typeNode in xml.SelectNodes("/PartCoverReport/type"))
            {
                CoverageReport.TypeDescriptor dType = new CoverageReport.TypeDescriptor();
                dType.assemblyName = GetStringAttribute(typeNode, "asm");
                dType.typeName = GetStringAttribute(typeNode, "name");
                dType.flags = GetUInt32Attribute(typeNode, "flags");

                foreach (XmlNode methodNode in typeNode.SelectNodes("method"))
                {
                    CoverageReport.MethodDescriptor dMethod = new CoverageReport.MethodDescriptor(0);
                    dMethod.methodName = GetStringAttribute(methodNode, "name");
                    dMethod.methodSig = GetStringAttribute(methodNode, "sig");
                    dMethod.flags = GetUInt32Attribute(methodNode, "flags");
                    dMethod.implFlags = GetUInt32Attribute(methodNode, "iflags");

                    foreach (XmlNode blockNode in methodNode.SelectNodes("code"))
                    {
                        CoverageReport.InnerBlockData dBlock = new CoverageReport.InnerBlockData();
                        foreach (XmlNode pointNode in blockNode.SelectNodes("pt"))
                        {
                            CoverageReport.InnerBlock dPoint = new CoverageReport.InnerBlock();
                            dPoint.visitCount = GetUInt32Attribute(pointNode, "visit");
                            dPoint.position = GetUInt32Attribute(pointNode, "pos");
                            dPoint.blockLen = GetUInt32Attribute(pointNode, "len");
                            if (pointNode.Attributes["fid"] != null)
                            {
                                dPoint.fileId = GetUInt32Attribute(pointNode, "fid");
                                dPoint.startLine = GetUInt32Attribute(pointNode, "sl");
                                dPoint.startColumn = GetUInt32Attribute(pointNode, "sc");
                                dPoint.endLine = GetUInt32Attribute(pointNode, "el");
                                dPoint.endColumn = GetUInt32Attribute(pointNode, "ec");
                            }
                            dBlock.AddBlock(dPoint);
                        }
                        dMethod.AddBlockData(dBlock);
                    }
                    dType.AddMethod(dMethod);
                }
                report.AddType(dType);
            }

            foreach (XmlNode messageNode in xml.SelectNodes("/PartCoverReport/run/tracker/message"))
                report.AddTrackerMessage(messageNode);

            foreach (XmlNode messageNode in xml.SelectNodes("/PartCoverReport/run/log/message"))
                report.AddLogFileMessage(messageNode);
        }

        private UInt32 GetUInt32Attribute(XmlNode node, string attr)
        {
            string strAttr = GetStringAttribute(node, attr);
            try
            {
                return UInt32.Parse(strAttr);
            }
            catch { throw new ReportException("Wrong report format, UInt32 type expected at " + node.Name + "[@" + attr + "]"); }
        }

        private int GetIntAttribute(XmlNode node, string attr)
        {
            string strAttr = GetStringAttribute(node, attr);
            try
            {
                return int.Parse(strAttr);
            }
            catch { throw new ReportException("Wrong report format, int type expected at " + node.Name + "[@" + attr + "]"); }
        }

        private string GetStringAttribute(XmlNode node, string attr)
        {
            XmlAttribute attrNode = node.Attributes[attr];
            if (attrNode == null || attrNode.Value == null) throw new ReportException("Wrong report format");
            return attrNode.Value;
        }
    }
}