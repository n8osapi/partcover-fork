
namespace PartCover.Browser.Api.ReportItems
{
    public interface IAssembly : IReportItem
    {
        IClass[] GetTypes();

        string Name { get;}
    }
}
