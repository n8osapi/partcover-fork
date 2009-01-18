using System;

namespace PartCover.Framework.Walkers
{
    public interface InstrumentedBlocksWalker
    {
        CoverageReport Report { get; }
    }

    internal class InstrumentedBlocksWalkerInner :
        InstrumentedBlocksWalker,
        IInstrumentedBlockWalker
    {
        private readonly CoverageReport coverage;
        private CoverageReport.AssemblyDescriptor currentAssembly;
        private CoverageReport.MethodDescriptor currentMethod;
        private CoverageReport.TypeDescriptor currentType;

        public InstrumentedBlocksWalkerInner()
        {
            coverage = new CoverageReport();
        }

        #region IInstrumentedBlockWalker Members

        public void BeginReport()
        {
            Report.start = DateTime.Now;
        }

        public void EndReport()
        {
            Report.end = DateTime.Now;
        }

        public void RegisterFile(UInt32 fileId, String fileUrl)
        {
            Report.AddFile(fileId, fileUrl);
        }

        public void EnterAssemblyDef(String assemblyName, String assemblyIdentity, String assemblyPath)
        {
            currentAssembly = new CoverageReport.AssemblyDescriptor(assemblyName);
            currentAssembly.assemblyIdentity = assemblyIdentity;
            currentAssembly.assemblyPath = assemblyPath;
        }

        public void LeaveAssembly()
        {
            Report.AddAssembly(currentAssembly);
        }

        public void EnterTypedef(String assemblyName, String typedefName, UInt32 flags)
        {
            currentType = new CoverageReport.TypeDescriptor();
            currentType.assemblyName = assemblyName;
            currentType.typeName = typedefName;
            currentType.flags = flags;
        }

        public void LeaveTypedef()
        {
            currentAssembly.AddType(currentType);
        }

        public void EnterMethod(String methodName, String methodSig, UInt32 flags, UInt32 implFlags)
        {
            currentMethod = new CoverageReport.MethodDescriptor(1);
            currentMethod.methodName = methodName;
            currentMethod.methodSig = methodSig;
            currentMethod.flags = flags;
            currentMethod.implFlags = implFlags;
        }

        public void LeaveMethod()
        {
            currentType.AddMethod(currentMethod);
        }

        public void MethodBlock(UInt32 position, UInt32 blockLen, UInt32 visitCount, UInt32 fileId, UInt32 startLine,
                                UInt32 startColumn, UInt32 endLine, UInt32 endColumn)
        {
            CoverageReport.InnerBlock inner = new CoverageReport.InnerBlock();
            inner.position = position;
            inner.blockLen = blockLen;
            inner.visitCount = visitCount;
            inner.fileId = fileId;
            inner.startLine = startLine;
            inner.startColumn = startColumn;
            inner.endLine = endLine;
            inner.endColumn = endColumn;

            currentMethod.AddMethodBlock(inner);
        }

        #endregion

        #region InstrumentedBlocksWalker Members

        public CoverageReport Report
        {
            get { return coverage; }
        }

        #endregion
    }
}