using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace PartCover.Framework.Walkers
{
    public sealed class CoverageReport
    {
        public int? ExitCode;
        public List<FileDescriptor> files = new List<FileDescriptor>();
        public List<RunHistoryMessage> runHistory = new List<RunHistoryMessage>();
        public List<RunLogMessage> runLog = new List<RunLogMessage>();
        public List<AssemblyDescriptor> assemblies = new List<AssemblyDescriptor>();

        public DateTime start;
        public DateTime end;

        public void AddAssembly(AssemblyDescriptor dAssembly)
        {
            AssemblyDescriptor existingAssembly = FindExistingAssembly(dAssembly);
            if (existingAssembly == null)
            {
                assemblies.Add(dAssembly);
                return;
            }

            AddAssemblies(dAssembly, existingAssembly);
        }

        public void AddFile(UInt32 id, String url)
        {
            FileDescriptor file = new FileDescriptor();
            file.fileId = id;
            file.fileUrl = url;
            files.Add(file);
        }

        public void AddType(TypeDescriptor dType)
        {
            TypeDescriptor existingType = FindExistingType(dType);
            if (existingType == null)
            {
                AssemblyDescriptor aDesc = FindExistingAssembly(dType.assemblyName);
                if (null == aDesc)
                {
                    aDesc = new AssemblyDescriptor(dType.assemblyName);
                    aDesc.AddType(dType);
                    AddAssembly(aDesc);
                } 
                else
                {
                    aDesc.AddType(dType);
                }
                return;
            }

            AddMethods(dType, existingType);
        }

        private void AddAssemblies(AssemblyDescriptor dAssembly, AssemblyDescriptor existingAssembly)
        {
            foreach (TypeDescriptor dType in dAssembly.types)
            {
                TypeDescriptor existingType = FindExistingType(dType);
                if (existingType == null)
                {
                    existingAssembly.AddType(dType);
                    return;
                }

                AddMethods(dType, existingType);
            }
        }

        private void AddMethods(TypeDescriptor newType, TypeDescriptor existingType)
        {
            foreach (MethodDescriptor md in newType.methods)
            {
                MethodDescriptor existingMethod = existingType.FindExistingMethod(md);
                if (existingMethod == null)
                {
                    MethodDescriptor[] newMethods = new MethodDescriptor[existingType.methods.Length + 1];
                    existingType.methods.CopyTo(newMethods, 1);
                    newMethods[0] = md;
                    existingType.methods = newMethods;
                    continue;
                }

                Debug.Assert(md.insBlocks.Length == 1);

                AddBlockData(md, existingMethod);
            }
        }

        private void AddBlockData(MethodDescriptor md, MethodDescriptor existingMethod)
        {
            InnerBlockData existingBlockData = existingMethod.FindExistingBlockData(md.insBlocks[0]);
            if (existingBlockData == null)
            {
                InnerBlockData[] newBlocks = new InnerBlockData[existingMethod.insBlocks.Length + 1];
                existingMethod.insBlocks.CopyTo(newBlocks, 1);
                newBlocks[0] = md.insBlocks[0];
                existingMethod.insBlocks = newBlocks;
            }
            else
            {
                for (int i = 0; i < existingBlockData.blocks.Length; ++i)
                {
                    InnerBlock existingBlock = existingBlockData.blocks[i];
                    InnerBlock newBlock = md.insBlocks[0].FindBlock(existingBlock);
                    Debug.Assert(newBlock != null);
                    existingBlock.visitCount += newBlock.visitCount;
                }
            }
        }

        public void AddLogFileMessage(XmlNode messageNode)
        {
            RunLogMessage message = new RunLogMessage();
            message.ThreadId = GetIntAttribute(messageNode, "tr");
            message.MsOffset = GetIntAttribute(messageNode, "ms");
            message.Message = messageNode.InnerText;
            runLog.Add(message);
        }

        public void AddTrackerMessage(XmlNode messageNode)
        {
            RunHistoryMessage message = new RunHistoryMessage();
            message.Time = new DateTime(GetLongAttribute(messageNode, "tm"), DateTimeKind.Utc);
            message.Message = messageNode.InnerText;
            runHistory.Add(message);
        }

        public TypeDescriptor FindExistingType(TypeDescriptor dType)
        {
            foreach (AssemblyDescriptor assembly in assemblies)
            {
                foreach (TypeDescriptor td in assembly.types)
                    if (td.assemblyName == dType.assemblyName && td.typeName == dType.typeName && td.flags == dType.flags)
                        return td;
            }
            
            return null;
        }

        public AssemblyDescriptor FindExistingAssembly(string assemblyName)
        {
            foreach (AssemblyDescriptor md in assemblies)
                if (md.name == assemblyName)
                    return md;
            return null;
        }

        public AssemblyDescriptor FindExistingAssembly(AssemblyDescriptor dAssembly)
        {
            return FindExistingAssembly(dAssembly.name);
        }

        public void ForEachInnerBlock(Action<InnerBlock> action)
        {
            assemblies.ForEach(
                delegate(AssemblyDescriptor assemDesc)
                    {
                        assemDesc.types.ForEach(
                            delegate(TypeDescriptor desc)
                                {
                                    Array.ForEach(desc.methods,
                                                  delegate(MethodDescriptor md)
                                                      {
                                                          Array.ForEach(md.insBlocks,
                                                                        delegate(InnerBlockData ib)
                                                                            { Array.ForEach(ib.blocks, action); });
                                                      });
                                });
                    });
            
        }

        public ICollection<TypeDescriptor> GetTypes(string assembly)
        {
            AssemblyDescriptor assem = assemblies.Find(delegate(AssemblyDescriptor desc) { return desc.name == assembly; });
            if (null != assem)
            {
                return assem.types;
            }

            return null;
        }

        public string[] GetAssemblies()
        {
            SortedList<String, Boolean> list = new SortedList<String, Boolean>();
            foreach (AssemblyDescriptor assemblyDescriptor in assemblies)
                list[assemblyDescriptor.name] = true;

            string[] res = new string[list.Count];
            list.Keys.CopyTo(res, 0);
            return res;
        }

        public string GetFileUrl(UInt32 fileId)
        {
            foreach (FileDescriptor fd in files)
                if (fd.fileId == fileId) return fd.fileUrl;
            return null;
        }

        private long GetLongAttribute(XmlNode node, string attr)
        {
            string strAttr = GetStringAttribute(node, attr);
            try
            {
                return long.Parse(strAttr);
            }
            catch { throw new ReportException("Wrong report format, long type expected at " + node.Name + "[@" + attr + "]"); }
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

        #region Nested type: FileDescriptor

        public class FileDescriptor
        {
            public UInt32 fileId;
            public string fileUrl;
        }

        #endregion

        #region Nested type: InnerBlock

        public class InnerBlock
        {
            public UInt32 blockLen;
            public UInt32 endColumn;
            public UInt32 endLine;
            public UInt32 fileId;
            public UInt32 position;
            public UInt32 startColumn;
            public UInt32 startLine;
            public UInt32 visitCount;

            public float GetBlockCoverage()
            {
                return visitCount > 0 ? 100 : 0;
            }
        }

        #endregion

        #region Nested type: InnerBlockData

        public class InnerBlockData
        {
            public readonly string uid = Guid.NewGuid().ToString("N");
            public InnerBlock[] blocks = new InnerBlock[0];


            public static UInt32 GetBlockCodeSize(InnerBlock[] blocks)
            {
                UInt32 codeSize = 0;
                foreach (InnerBlock ib in blocks)
                    codeSize += ib.blockLen;
                return codeSize;
            }

            public UInt32 GetBlockCodeSize()
            {
                return GetBlockCodeSize(blocks);
            }

            public static UInt32 GetBlockCoveredCodeSize(InnerBlock[] blocks)
            {
                UInt32 codeSize = 0;
                foreach (InnerBlock ib in blocks)
                    if (ib.visitCount > 0) codeSize += ib.blockLen;
                return codeSize; 
            }


            public UInt32 GetBlockCoveredCodeSize()
            {
                return GetBlockCoveredCodeSize(blocks);
            }

            public void AddBlock(InnerBlock inner)
            {
                InnerBlock[] newBlocks = new InnerBlock[blocks.Length + 1];
                blocks.CopyTo(newBlocks, 1);
                newBlocks[0] = inner;
                blocks = newBlocks;
            }

            public InnerBlock FindBlock(InnerBlock block)
            {
                foreach (InnerBlock dataBlock in blocks)
                {
                    if (dataBlock.position != block.position || dataBlock.blockLen != block.blockLen)
                        continue;
                    if (dataBlock.fileId == block.fileId &&
                        dataBlock.startLine == block.startLine && dataBlock.startColumn == block.startColumn &&
                        dataBlock.endLine == block.endLine && dataBlock.endColumn == block.endColumn)
                        return dataBlock;
                }
                return null;
            }
        }

        #endregion

        #region Nested type: MethodDescriptor

        public class MethodDescriptor
        {
            public UInt32 flags;
            public UInt32 implFlags;

            public InnerBlockData[] insBlocks;
            public string methodName;
            public string methodSig;

            public MethodDescriptor(int initialBlockSize)
            {
                SetBlockDataSize(initialBlockSize);
            }

            public MethodDescriptor()
            {
                SetBlockDataSize(0);
            }

            public void AddMethodBlock(InnerBlock inner)
            {
                InnerBlockData bData = insBlocks[0];
                InnerBlock[] newBlocks = new InnerBlock[bData.blocks.Length + 1];
                bData.blocks.CopyTo(newBlocks, 1);
                newBlocks[0] = inner;
                bData.blocks = newBlocks;
            }

            public void AddBlockData(InnerBlockData bData)
            {
                InnerBlockData[] newBlocks = new InnerBlockData[insBlocks.Length + 1];
                insBlocks.CopyTo(newBlocks, 1);
                newBlocks[0] = bData;
                insBlocks = newBlocks;
            }

            public InnerBlockData FindExistingBlockData(InnerBlockData bData)
            {
                foreach (InnerBlockData dataBlock in insBlocks)
                {
                    if (dataBlock.blocks.Length != bData.blocks.Length)
                        continue;
                    bool validBlock = true;
                    for (int i = 0; validBlock && i < dataBlock.blocks.Length; ++i)
                    {
                        InnerBlock existingBlock = dataBlock.blocks[i];
                        InnerBlock newBlock = bData.FindBlock(existingBlock);
                        validBlock = newBlock != null;
                    }
                    if (validBlock)
                        return dataBlock;
                }
                return null;
            }

            public UInt32 GetCodeSize(int blockIndex)
            {
                UInt32 res = 0;
                foreach (InnerBlock inner in insBlocks[blockIndex].blocks)
                    res += inner.blockLen;
                return res;
            }

            public UInt32 GetCoveredCodeSize(int blockIndex)
            {
                UInt32 res = 0;
                foreach (InnerBlock inner in insBlocks[blockIndex].blocks)
                    if (inner.visitCount > 0) res += inner.blockLen;
                return res;
            }

            private void SetBlockDataSize(int initialBlockSize)
            {
                insBlocks = new InnerBlockData[initialBlockSize];
                while (initialBlockSize-- > 0)
                    insBlocks[initialBlockSize] = new InnerBlockData();
            }
        }

        #endregion

        #region Nested type: AssemblyDescriptor

        public class AssemblyDescriptor
        {
            public string name;
            public string assemblyPath;
            public string assemblyIdentity;

            public List<TypeDescriptor> types = new List<TypeDescriptor>();

            public AssemblyDescriptor(string assemblyName)
            {
                name = assemblyName;
            }

            public void AddType(TypeDescriptor typeDescriptor)
            {
                types.Add(typeDescriptor);
            }
        }

        #endregion

        #region Nested type: RunHistoryMessage

        public class RunHistoryMessage
        {
            public String Message;
            public DateTime Time;
        }

        #endregion

        #region Nested type: RunLogMessage

        public class RunLogMessage
        {
            public string Message;
            public int MsOffset;
            public int ThreadId;
        }

        #endregion

        #region Nested type: TypeDescriptor

        public class TypeDescriptor
        {
            public string assemblyName;
            public UInt32 flags;

            public MethodDescriptor[] methods = new MethodDescriptor[0];
            public string typeName;

            public void AddMethod(MethodDescriptor dMethod)
            {
                MethodDescriptor[] newMethods = new MethodDescriptor[methods.Length + 1];
                methods.CopyTo(newMethods, 1);

                newMethods[0] = dMethod;

                methods = newMethods;
            }

            public MethodDescriptor FindExistingMethod(MethodDescriptor dMethod)
            {
                foreach (MethodDescriptor md in methods)
                    if (md.methodName == dMethod.methodName && md.methodSig == dMethod.methodSig && md.flags == dMethod.flags && md.implFlags == dMethod.implFlags)
                        return md;
                return null;
            }
        }

        #endregion
    }
}