using System.Collections.Generic;

namespace SheetProcessor
{
    public interface ISheetPrinterData
    {
        IReadOnlyList<string> VerticalSheetName { get; }
        IReadOnlyList<string> HorizontalSheetName { get; }
        IReadOnlyDictionary<ICellPosition, string> CellData { get; }
        string FetchData(string row, string column);
    }
}
