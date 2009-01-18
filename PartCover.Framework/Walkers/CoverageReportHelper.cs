
namespace PartCover.Framework.Walkers
{
    public static class CoverageReportHelper
    {
        public static string[] SplitNamespaces(string typedefName)
        {
            return typedefName.Split('.');
        }

        public static string GetTypeDefName(string typedefName)
        {
            string[] names = SplitNamespaces(typedefName);
            return names[names.Length - 1];
        }
    }
}
