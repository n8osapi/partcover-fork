
namespace PartCover.Browser.Api.ReportItems
{
    public interface INamespace : IReportItem
    {
        string Name { get;}

        INamespace Parent { get;}

        IAssembly Assembly { get;}
    }
}
