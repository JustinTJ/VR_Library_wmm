using System.Collections;
using System.Collections.Generic;

namespace SheetProcessor
{
    public class FeedbackSheetData : ISheetData
    {
        
        public List<string> VerticalSheetName { get; set; } = new();
        public List<string> HorizontalSheetName { get; set; } = new();
        public Dictionary<ICellPosition, string> CellData { get; set;  } = new();

        public ISheetPrinterData GetPrinterData()
        {
            return new FeedbackSheetPrinterData(VerticalSheetName, HorizontalSheetName , CellData);
        }
    }
}