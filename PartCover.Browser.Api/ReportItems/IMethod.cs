
namespace PartCover.Browser.Api.ReportItems
{
    public interface IMethod : IReportItem
    {
        uint Flags { get; }
        string Name { get;}

        IClass Class { get;}

        ICoveredVariant[] CoveredVariants { get;}
    }
}
