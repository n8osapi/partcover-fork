using System;
using System.Collections.Generic;
using PartCover.Browser.Api;
using PartCover.Browser.Api.ReportItems;
using PartCover.Browser.Stuff.ReportItems;
using PartCover.Framework.Walkers;

namespace PartCover.Browser.Stuff
{
    internal class CoverageReportWrapper : ICoverageReport
    {
        private readonly List<AssemblyItem> assemblyList = new List<AssemblyItem>();
        private readonly CoverageReport report;

        public CoverageReportWrapper(CoverageReport report)
        {
            this.report = report;
        }

        public CoverageReport Report
        {
            get { return report; }
        }

        #region ICoverageReport Members

        public IAssembly[] GetAssemblies()
        {
            return assemblyList.ToArray();
        }

        public string GetFilePath(uint file)
        {
            return report.GetFileUrl(file);
        }

        public void ForEachBlock(Action<CoverageReport.InnerBlock> blockReceiver)
        {
            report.ForEachInnerBlock(blockReceiver);
        }

        public ICollection<CoverageReport.RunHistoryMessage> GetRunHistory()
        {
            return report.runHistory;
        }

        public ICollection<CoverageReport.RunLogMessage> GetLogEvents()
        {
            return report.runLog;
        }

        public int? GetExitCode()
        {
            return report.ExitCode;
        }

        #endregion

        public void Build()
        {
            foreach (string asmName in report.GetAssemblies())
            {
                AssemblyItem assemblyItem = new AssemblyItem(asmName);
                foreach (CoverageReport.TypeDescriptor d in report.GetTypes(assemblyItem.Name))
                {
                    ClassItem classItem = new ClassItem(d.typeName, assemblyItem);
                    BuildNamespaceChain(assemblyItem, classItem);
                    BuildMethods(d.methods, classItem);
                    assemblyItem.AddType(classItem);
                }
                assemblyList.Add(assemblyItem);
            }
        }

        private void BuildMethods(CoverageReport.MethodDescriptor[] mdList, ClassItem classItem)
        {
            foreach (CoverageReport.MethodDescriptor md in mdList)
            {
                MethodItem mdItem = new MethodItem(md, classItem);
                BuildMethodBlocks(md, mdItem);
                classItem.AddMethod(mdItem);
            }
        }

        private void BuildMethodBlocks(CoverageReport.MethodDescriptor md, MethodItem mdItem)
        {
            foreach (CoverageReport.InnerBlockData ibd in md.insBlocks)
            {
                CoveredVariantItem cvItem = new CoveredVariantItem();
                cvItem.Blocks = ibd.blocks;
                mdItem.AddBlock(cvItem);
            }
        }

        private void BuildNamespaceChain(AssemblyItem assemblyItem, ClassItem classItem)
        {
            string[] parts = CoverageReportHelper.SplitNamespaces(classItem.QName);

            NamespaceItem lastNamespaceItem = null;
            for (int i = 0; i < parts.Length - 1; ++i)
            {
                NamespaceItem namespaceItem = assemblyItem.FindNamespace(parts[i], lastNamespaceItem);
                if (namespaceItem == null)
                {
                    namespaceItem = new NamespaceItem(parts[i], assemblyItem);
                    namespaceItem.Parent = lastNamespaceItem;
                    assemblyItem.AddNamespace(namespaceItem);
                }
                lastNamespaceItem = namespaceItem;
            }

            classItem.Namespace = lastNamespaceItem;
        }
    }
}