using System;
using System.Xml;

namespace PartCover.Framework.Settings
{
    public class WorkSettingsReader
    {
        public WorkSettings ReadSettingsFile(string settingsFile)
        {
            WorkSettings settings = new WorkSettings();
            settings.SettingsFile = settingsFile;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(settingsFile);
                settings.LogLevel = 0;

                XmlNode node = xmlDoc.SelectSingleNode("/PartCoverSettings/Target/text()");
                if (node != null && node.Value != null) settings.TargetPath = node.Value;
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/TargetWorkDir/text()");
                if (node != null && node.Value != null) settings.TargetWorkingDir = node.Value;
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/TargetArgs/text()");
                if (node != null && node.Value != null) settings.TargetArgs = node.Value;
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/LogLevel/text()");
                if (node != null && node.Value != null) settings.LogLevel = int.Parse(node.Value);
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/Output/text()");
                if (node != null && node.Value != null) settings.FileNameForReport = node.Value;
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/ShowHelp/text()");
                if (node != null && node.Value != null) settings.PrintLongHelp = bool.Parse(node.Value);
                node = xmlDoc.SelectSingleNode("/PartCoverSettings/ShowVersion/text()");
                if (node != null && node.Value != null) settings.PrintVersion = bool.Parse(node.Value);

                XmlNodeList list = xmlDoc.SelectNodes("/PartCoverSettings/Rule");
                if (list != null)
                {
                    foreach (XmlNode rule in list)
                    {
                        XmlNode ruleText = rule.SelectSingleNode("text()");
                        if (ruleText == null || string.IsNullOrEmpty(ruleText.Value))
                            continue;
                        string[] rules = ruleText.Value.Split(',');
                        foreach (string s in rules)
                        {
                            if (s.Length <= 1)
                                continue;
                            if (s[0] == '+')
                                settings.IncludeRules(s.Substring(1));
                            else if (s[0] == '-')
                                settings.ExcludeRules(s.Substring(1));
                            else
                                throw new SettingsException("Wrong rule format (" + s + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SettingsException("Cannot load settings (" + ex.Message + ")");
            }

            return settings;
        }
    }
}
