
namespace PartCover.Browser.Api.ReportItems
{
    public interface IClass : IReportItem
    {
        uint Flags { get;}

        string Name { get;}

        IAssembly Assembly { get;}

        IMethod[] GetMethods();

        INamespace[] GetNamespaceChain();
    }
}
