using System.Collections.Generic;

namespace SheetProcessor
{
    public interface ISheetData
    {
        List<string> VerticalSheetName { get; set; }
        List<string> HorizontalSheetName { get; set; }
        Dictionary<ICellPosition, string> CellData { get; set; }

        ISheetPrinterData GetPrinterData();
    }
}