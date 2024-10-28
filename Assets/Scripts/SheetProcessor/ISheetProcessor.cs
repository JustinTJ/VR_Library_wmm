namespace SheetProcessor
{
    public interface ISheetProcessor
    {
        void PrintSheet(ISheetPrinterData sheetPrinterData, ISheetPrinter sheetPrinter);
    }
}