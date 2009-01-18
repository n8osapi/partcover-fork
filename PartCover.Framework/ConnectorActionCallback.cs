using PartCover.Framework.Walkers;

namespace PartCover.Framework
{
    public class ConnectorActionCallback : IConnectorActionCallback
    {
        private readonly Connector connector;

        public ConnectorActionCallback(Connector connector)
        {
            this.connector = connector;
        }

        void IConnectorActionCallback.DriverConnected() { connector.ProcessCallback.WriteStatus("driver connected successfully"); }

        void IConnectorActionCallback.DriverSendRules() { connector.ProcessCallback.WriteStatus("send rules to the driver"); }

        void IConnectorActionCallback.DriverWaitEoIConfirm() { connector.ProcessCallback.WriteStatus("wait for rules confirm"); }

        void IConnectorActionCallback.FunctionsCount(uint count) { connector.ProcessCallback.WriteStatus(string.Format("functions count {0}", count)); }

        void IConnectorActionCallback.FunctionsReceiveBegin() { connector.ProcessCallback.WriteStatus("functions loading is being started"); }

        void IConnectorActionCallback.FunctionsReceiveEnd() { connector.ProcessCallback.WriteStatus("functions loading is complete"); }

        void IConnectorActionCallback.FunctionsReceiveStat(uint index) { connector.ProcessCallback.WriteStatus(string.Format("functions loaded: {0}", index)); }

        void IConnectorActionCallback.InstrumentDataReceiveBegin() { connector.ProcessCallback.WriteStatus("instrument data is being received"); }

        void IConnectorActionCallback.InstrumentDataReceiveCountersAsm(string name, string mod, uint typeDefCount) { connector.ProcessCallback.WriteStatus(string.Format("load assembly {0} ({1} types)", name, typeDefCount)); }

        void IConnectorActionCallback.InstrumentDataReceiveCountersAsmCount(uint asmCount) { connector.ProcessCallback.WriteStatus(string.Format("assembly count: {0}", asmCount)); }

        void IConnectorActionCallback.InstrumentDataReceiveCountersBegin() { connector.ProcessCallback.WriteStatus("InstrumentDataReceiveCountersBegin"); }

        void IConnectorActionCallback.InstrumentDataReceiveCountersEnd() { connector.ProcessCallback.WriteStatus("InstrumentDataReceiveCountersEnd"); }

        void IConnectorActionCallback.InstrumentDataReceiveEnd() { connector.ProcessCallback.WriteStatus("instrument data load complete"); }

        void IConnectorActionCallback.InstrumentDataReceiveFilesBegin() { connector.ProcessCallback.WriteStatus("file list is being received"); }

        void IConnectorActionCallback.InstrumentDataReceiveFilesCount(uint fileCount) { connector.ProcessCallback.WriteStatus(string.Format("{0} files to load", fileCount)); }

        void IConnectorActionCallback.InstrumentDataReceiveFilesEnd() { connector.ProcessCallback.WriteStatus("file list load is complete"); }

        void IConnectorActionCallback.InstrumentDataReceiveFilesStat(uint index) { }

        void IConnectorActionCallback.InstrumentDataReceiveStatus() { connector.ProcessCallback.WriteStatus("driver connected"); }

        void IConnectorActionCallback.MethodsReceiveBegin() { connector.ProcessCallback.WriteStatus("function map is being received"); }

        void IConnectorActionCallback.MethodsReceiveEnd() { connector.ProcessCallback.WriteStatus("function map load complete"); }

        void IConnectorActionCallback.MethodsReceiveStatus() { connector.ProcessCallback.WriteStatus("function map load status"); }

        void IConnectorActionCallback.OpenMessagePipe() { connector.ProcessCallback.WriteStatus("open driver pipe"); }

        void IConnectorActionCallback.SetConnected(bool connected) { connector.ProcessCallback.WriteStatus(connected ? "driver connected" : "driver disconnected"); }

        void IConnectorActionCallback.TargetCreateProcess() { connector.ProcessCallback.WriteStatus("create target process"); }

        void IConnectorActionCallback.TargetRequestShutdown() { connector.ProcessCallback.WriteStatus("request target shutdown"); }

        void IConnectorActionCallback.TargetSetEnvironmentVars() { connector.ProcessCallback.WriteStatus("modify target environment variables"); }

        void IConnectorActionCallback.TargetWaitDriver() { connector.ProcessCallback.WriteStatus("wait for driver connection"); }

        void IConnectorActionCallback.LogMessage(int threadId, int tick, string text)
        {
            CoverageReport.RunLogMessage message = new CoverageReport.RunLogMessage();
            message.Message = text;
            message.MsOffset = tick;
            message.ThreadId = threadId;
            connector.OnLogMessage(message);
        }
    }
}
