using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace PartCover.Framework.Settings
{
    public class WorkSettingsWriter
    {
        public void GenerateSettingsFile(WorkSettings settings)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateElement("PartCoverSettings"));
            if (settings.TargetPath != null) AppendValue(xmlDoc.DocumentElement, "Target", settings.TargetPath);
            if (settings.TargetWorkingDir != null) AppendValue(xmlDoc.DocumentElement, "TargetWorkDir", settings.TargetWorkingDir);
            if (settings.TargetArgs != null) AppendValue(xmlDoc.DocumentElement, "TargetArgs", settings.TargetArgs);
            if (settings.LogLevel > 0)
                AppendValue(xmlDoc.DocumentElement, "LogLevel", settings.LogLevel.ToString(CultureInfo.InvariantCulture));
            if (settings.SettingsFile != null) AppendValue(xmlDoc.DocumentElement, "Output", settings.SettingsFile);
            if (settings.PrintLongHelp)
                AppendValue(xmlDoc.DocumentElement, "ShowHelp", settings.PrintLongHelp.ToString(CultureInfo.InvariantCulture));
            if (settings.PrintVersion)
                AppendValue(xmlDoc.DocumentElement, "ShowVersion", settings.PrintVersion.ToString(CultureInfo.InvariantCulture));

            foreach (string item in settings.IncludeItems) AppendValue(xmlDoc.DocumentElement, "Rule", "+" + item);
            foreach (string item in settings.ExcludeItems) AppendValue(xmlDoc.DocumentElement, "Rule", "-" + item);

            try
            {
                if ("console".Equals(settings.GenerateSettingsFileName, StringComparison.InvariantCulture))
                    xmlDoc.Save(Console.Out);
                else
                    xmlDoc.Save(settings.GenerateSettingsFileName);
            }
            catch (Exception ex)
            {
                throw new SettingsException("Cannot write settings (" + ex.Message + ")");
            }
        }

        private static void AppendValue(XmlNode parent, string name, string value)
        {
            Debug.Assert(parent != null && parent.OwnerDocument != null);
            XmlNode node = parent.AppendChild(parent.OwnerDocument.CreateElement(name));
            node.InnerText = value;
        }
    }
}
