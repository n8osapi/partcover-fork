using System;

namespace PartCover.Framework.Settings
{
    public class SettingsException : Exception
    {
        public SettingsException(string message)
            : base(message)
        {
        }
    }
}
