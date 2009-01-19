using System;

namespace PartCover.Browser.Api
{
    public interface IReportItemSelectionService
    {
        void select<T>(T asm) where T : IReportItem;

        void selectNone();

        IReportItem SelectedItem { get;}

        event EventHandler SelectionChanged;
    }
}
