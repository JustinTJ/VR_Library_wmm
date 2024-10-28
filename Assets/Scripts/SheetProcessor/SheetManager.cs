using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Feedback;
using UnityEngine;
using UnityEngine.Serialization;

namespace SheetProcessor
{
    public class SheetManager : SingletonMono<SheetManager>
    {
        public string TimeLabel;
        
        private readonly List<SingleColumnSheetMono> _sheetMonos = new List<SingleColumnSheetMono>(); 
        
        [SerializeField]
        private FeedbackSheetSetting _feedbackSheetSetting;

        public string CurrentLocation = "MetroTicketMachinesArea";

        private void Start()
        {
            var label = DateTime.Now;
            TimeLabel = label.Year.ToString() + label.Month + label.Day + label.Hour + label.Minute + label.Second;
        }

        public void AddSheetMono(SingleColumnSheetMono sheetMono)
        {
            _sheetMonos.Add(sheetMono);
        }

        public void SaveCsv()
        {
            ISheetData sheet = new FeedbackSheetData();
            sheet.HorizontalSheetName = _feedbackSheetSetting.horizontalNames;
            sheet.VerticalSheetName = _feedbackSheetSetting.verticalNames;
            Dictionary<ICellPosition,StringBuilder> cells = new Dictionary<ICellPosition, StringBuilder>();
            Debug.Log("sheet mono size" + _sheetMonos.Count + " hor size" + sheet.HorizontalSheetName.Count);
            foreach (var sheetMono in _sheetMonos)
            {
                for (int rowIndex = 0; rowIndex < sheet.VerticalSheetName.Count; rowIndex++)
                {
                    ICellPosition position = new FeedbackCellPosition(sheet.VerticalSheetName[rowIndex],
                        sheetMono.locationName);
                    if (!cells.ContainsKey(position))
                        cells[position] = new StringBuilder();
                    cells[position].Append("[" + sheetMono.GetRowData(rowIndex) + "]");
                }
            }

            foreach (var cellPair in cells)
            {
                sheet.CellData.Add(cellPair.Key,cellPair.Value.ToString());
            }
            ISheetProcessor processor = new FeedbackProcessor();
            processor.PrintSheet(sheet.GetPrinterData(),
                new CsvFormatSheetPrinter(Application.persistentDataPath, "feedback_sheet"));
        }
    }
}
