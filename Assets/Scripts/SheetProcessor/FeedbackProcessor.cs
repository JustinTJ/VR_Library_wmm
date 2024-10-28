namespace SheetProcessor
{
    public class FeedbackProcessor:ISheetProcessor
    {
        public void PrintSheet(ISheetPrinterData sheetPrinterData, ISheetPrinter sheetPrinter)
        {
            sheetPrinter.PrintSheet(sheetPrinterData);
        }
    }
}