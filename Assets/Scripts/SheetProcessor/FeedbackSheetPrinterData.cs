using System.Collections.Generic;

namespace SheetProcessor
{
    public class FeedbackSheetPrinterData : ISheetPrinterData
    {
        public IReadOnlyList<string> VerticalSheetName { get; }
        public IReadOnlyList<string> HorizontalSheetName { get; }
        public IReadOnlyDictionary<ICellPosition, string> CellData { get; }

        public FeedbackSheetPrinterData(IReadOnlyList<string> verticalSheetName, IReadOnlyList<string> horizontalSheetName, IReadOnlyDictionary<ICellPosition, string> cellData)
        {
            VerticalSheetName = verticalSheetName;
            HorizontalSheetName = horizontalSheetName;
            CellData = cellData;
        }
        public string FetchData(string row, string column)
        {
            if (CellData.TryGetValue(new FeedbackCellPosition(row, column), out var data))
            {
                return data;
            }
            return string.Empty;
        }
    }
}