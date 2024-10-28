using System;
using System.IO;
using UnityEngine;

namespace SheetProcessor
{
    public class CsvFormatSheetPrinter : ISheetPrinter
    {
        private string FullPath { get; set; }

        public CsvFormatSheetPrinter(string path, string fileName)
        {
            var label = SheetManager.Instance.TimeLabel;
            FullPath = path + fileName + "_"+ label + ".csv";
        }


        public void PrintSheet(ISheetPrinterData sheetPrinterData)
        {
            var success = true;
            if (File.Exists(FullPath))
            {
                try
                {
                    File.Delete(FullPath);

                }
                catch (Exception e)
                {
                    Debug.LogWarning("The Excel "+ FullPath +" has been busy, please close it and try again.");
                    success = false;
                    // ignored
                }
            }

            if (!success)
            {
                return;
            }
            Debug.Log(FullPath);
            using (TextWriter writer = File.CreateText(FullPath)) {
                for (var columnIndex = 0;
                     columnIndex < sheetPrinterData.HorizontalSheetName.Count;
                     columnIndex++)
                {
                    writer.Write("," + sheetPrinterData.HorizontalSheetName[columnIndex]);
                }
                writer.Write("\r\n");
                for (var rowIndex = 0; rowIndex < sheetPrinterData.VerticalSheetName.Count; rowIndex++)
                {
                    var row = sheetPrinterData.VerticalSheetName[rowIndex];
                    writer.Write(sheetPrinterData.VerticalSheetName[rowIndex]);
                    for (var columnIndex = 0; columnIndex < sheetPrinterData.HorizontalSheetName.Count; columnIndex++)
                    {
                        var column = sheetPrinterData.HorizontalSheetName[columnIndex];
                        writer.Write("," + sheetPrinterData.FetchData(row,column));
                    }
                    writer.Write("\r\n");
                }
            }
        }
    }
}