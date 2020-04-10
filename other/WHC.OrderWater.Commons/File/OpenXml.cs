using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace Common
{
    public class IncOpenExcel : IDisposable
    {
        public readonly static string error_filetype = "it is not .xlsx";
        public readonly static string error_empty = "it is null or empty";

        private SpreadsheetDocument document = null;
        public DefaultCellStyle defaultCellStyle = null;


        #region public
        #region file
        public bool IsValidate()
        {
            return document == null ? false : true;
        }
        public IncOpenExcel(string FilePath, string sheetName, bool isCreate)
        {
            if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(sheetName))
            {
                throw new Exception(error_empty);
            }
            if (Path.GetExtension(FilePath) != ".xlsx")
            {
                throw new Exception(error_filetype);
            }

            if (isCreate)
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
                document = CreateFile(FilePath, sheetName, out defaultCellStyle);
            }
            else
            {
                document = openFile(FilePath, out defaultCellStyle);
            }
        }

        public void ClearFormulaCache(string sheetName)
        {
            SheetData sheetData = GetSheetData(sheetName);
            if (sheetData != null)
            {
                var rows = sheetData.Elements<Row>();
                foreach (Row row in rows)
                {
                    if (row != null)
                    {
                        foreach (Cell cell in row.Elements<Cell>())
                        {
                            if (cell.CellFormula != null)
                            {
                                cell.CellValue = null;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region row
        public bool CreateOrUpdateRowAt(string sheetName, DataRow dataRow, uint rowNo, uint columnNo, Dictionary<uint, uint> eachColumnStyle = null)
        {
            bool res = false;
            SheetData sheetData = GetSheetData(sheetName);
            if (sheetData != null)
            {
                res = IncOpenExcel.CreateOrUpdateRowAt(sheetData, dataRow, rowNo, columnNo, defaultCellStyle, eachColumnStyle);
            }
            return res;
        }

        public IEnumerable<Row> GetRows(string sheetName, uint startRowNo, uint endRowNo)
        {
            IEnumerable<Row> res = null;
            if (!string.IsNullOrEmpty(sheetName))
            {
                SheetData sheetdata = GetSheetData(sheetName);
                res = IncOpenExcel.GetRows(sheetdata, startRowNo, endRowNo);
            }
            return res;
        }


        public Row GetRow(string sheetName, uint RowNo)
        {
            Row res = null;
            if (!string.IsNullOrEmpty(sheetName))
            {
                SheetData sheetdata = GetSheetData(sheetName);
                var rows = sheetdata.Elements<Row>().Where(x => x.RowIndex == RowNo);
                if (rows.Count() > 0)
                { res = rows.First(); }
            }
            return res;
        }


        public List<string> GetRowsXml(string sheetName, uint startRowNo, uint endRowNo)
        {
            List<string> res = null;
            if (!string.IsNullOrEmpty(sheetName))
            {
                var sheetdata = GetSheetData(sheetName);
                res = IncOpenExcel.GetRowsXml(sheetdata, startRowNo, endRowNo);
            }
            return res;
        }

        public bool DeleteRows(string sheetName, uint startRowNo, uint endRowNo)
        {
            bool res = false;
            var rows = GetRows(sheetName, startRowNo, endRowNo);
            if (rows != null)
            {
                IncOpenExcel.DeleteRows(rows);
            }
            res = true;
            return res;
        }

        public bool MoveRows(string sheetName, List<string> xmlrows, uint newLineNo)
        {
            bool res = false;
            SheetData sheetData = GetSheetData(sheetName);
            List<Row> newRowsList = new List<Row>();
            if (xmlrows != null)
            {
                int index = 0;
                foreach (string strRow in xmlrows)
                {
                    try
                    {
                        Row changingRow = new Row(strRow);
                        ChangeRowNoAndCellReference(changingRow, newLineNo + (uint)index);
                        newRowsList.Add(changingRow);
                    }
                    catch
                    {
                        newRowsList = new List<Row>();
                    }
                    index++;
                }
            }

            //insert into datasheet.
            foreach (Row row in newRowsList)
            {
                CreateOrUpdateRowAt(sheetData, row, row.RowIndex);
            }

            return res;
        }

        #endregion

        #region cell
        public bool CreateOrUpdateCellAt(string sheetName, uint rowNumber, uint columnNumber, Type datatype, object value, uint customStyle = 0)
        {
            if (!string.IsNullOrEmpty(sheetName))
            {
                SheetData sheetData = GetSheetData(sheetName);
                if (sheetData != null)
                {
                    SetOrUpdateCellValue(sheetData, rowNumber, columnNumber, datatype, value, defaultCellStyle, customStyle);
                }
            }
            return true;
        }
        #endregion

        #region other
        public static Dictionary<uint, uint> getRowStyles(Row theRow)
        {
            Dictionary<uint, uint> styles = new Dictionary<uint, uint>();
            if (theRow != null)
            {
                var cells = theRow.Elements<Cell>();
                foreach (Cell cell in cells)
                {
                    if (!string.IsNullOrEmpty(cell.CellReference) && cell.StyleIndex != null)
                    {
                        uint column;
                        Ref2RC(cell.CellReference, out column);
                        styles.Add(column, cell.StyleIndex);
                    }
                }
            }
            return styles;
        }

        //0:normal 1. 4lines  2. balck  3black
        public static Dictionary<uint, uint> getRowStyles(DataColumnCollection dataColumn, uint startColumnNo, int cateogry, DefaultCellStyle defaultCellStyle)
        {
            Dictionary<uint, uint> styles = new Dictionary<uint, uint>();
            if (dataColumn != null)
            {
                for (int i = 0; i < dataColumn.Count; i++)
                {
                    uint defaultStyle = IncOpenExcel.getDefaultStyle(dataColumn[i].DataType, defaultCellStyle, cateogry);
                    styles.Add((uint)i + startColumnNo, defaultStyle);
                }
            }
            return styles;
        }

        //category .0 normal 1 border  2 black  3black and border
        public static uint getDefaultStyle(System.Type type, DefaultCellStyle defaultCellStyle, int category)
        {
            uint res = 0;
            CellValues theCellValue = GetValueType(type);

            if (theCellValue == CellValues.Date)
            {
                if (category == 0)
                {
                    res = defaultCellStyle.dateTimeIndex;
                }
                else if (category == 1)
                {
                    res = defaultCellStyle.blackdateTimeIndex;
                }
                else if (category == 2)
                {
                    res = defaultCellStyle.dateTimeIndex_border;
                }
                else if (category == 3)
                {
                    res = defaultCellStyle.blackdateTimeIndex_border;
                }
            }
            else
            {
                if (category == 0)
                {
                    res = defaultCellStyle.normalIndex;
                }
                else if (category == 1)
                {
                    res = defaultCellStyle.blackIndex;
                }
                else if (category == 2)
                {
                    res = defaultCellStyle.normalIndex_border;
                }
                else if (category == 3)
                {
                    res = defaultCellStyle.blackIndex_border;
                }

            }
            return res;
        }


        public static void Ref2RC(string refa, out uint columnNo)
        {
            columnNo = 0;
            refa = refa.ToUpper();
            Dictionary<char, int> maping = new Dictionary<char, int>();
            maping.Add('A', 1); maping.Add('B', 2); maping.Add('C', 3); maping.Add('D', 4);
            maping.Add('E', 5); maping.Add('F', 6); maping.Add('G', 7); maping.Add('H', 8);
            maping.Add('I', 9); maping.Add('J', 10); maping.Add('K', 11); maping.Add('L', 12);
            maping.Add('M', 13); maping.Add('N', 14); maping.Add('O', 15); maping.Add('P', 16);
            maping.Add('Q', 17); maping.Add('R', 18); maping.Add('S', 19); maping.Add('T', 20);
            maping.Add('U', 21); maping.Add('V', 22); maping.Add('W', 23); maping.Add('X', 24);
            maping.Add('Y', 25); maping.Add('Z', 26);

            List<char> chars = refa.ToList<char>();
            List<char> columns = new List<char>();
            foreach (char c in chars)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    columns.Add(c);
                }
            }
            if (columns.Count == 2)
            {
                columnNo = (uint)(26 * maping[columns[0]] + maping[columns[1]]);
            }
            else if (columns.Count == 1)
            {
                columnNo = (uint)maping[columns[0]];
            }
        }

        public uint FindStringInColumn(string sheetname, string str, uint columnNo)
        {
            uint res = 0;
            SheetData sheetData = GetSheetData(sheetname);
            if (sheetData != null)
            {
                IEnumerable<Row> rows = sheetData.Elements<Row>();
                foreach (Row row in rows)
                {
                    if (row.Elements<Cell>() != null && row.Elements<Cell>().Count() > 0)
                    {
                        var Cells = row.Elements<Cell>().Where(x => x.CellReference.Value == GetCellReference(row.RowIndex, columnNo));
                        if (Cells != null && Cells.Count() > 0 && GetCellRealString(document, Cells.First()) == str)
                        {
                            res = row.RowIndex;
                        }
                    }
                }
            }
            return res;
        }


        public Worksheet getWorksheet(string sheetname)
        {
            Worksheet res = null;
            IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetname);
            if (sheets.Count() > 0)
            {
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                res = worksheetPart.Worksheet;
            }
            return res;
        }
        public SheetData GetSheetData(string sheetname)
        {
            Worksheet worksheet = getWorksheet(sheetname);
            return GetSheetData(worksheet);
        }
        public SheetData GetSheetData(Worksheet worksheet)
        {
            SheetData res = null;
            if (worksheet != null)
            {
                var sds = worksheet.Elements<SheetData>();
                if (sds != null && sds.Count() > 0)
                {
                    res = sds.First();
                }
            }
            return res;
        }
        public void SaveAndClose()
        {
            if (document != null)
            {
                document.Close();
            }
        }
        public void Dispose()
        {
            if (document != null)
            {
                document.Close();
            }
        }
        #endregion
        #endregion 

        #region private
        #region file
        private static SpreadsheetDocument CreateFile(string newFilePath, string firstSheetName, out DefaultCellStyle defaultCellStyle)
        {
            //建立xlsx文件
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(newFilePath, SpreadsheetDocumentType.Workbook, true);

            //建立xl,worksheets目录(会默认生成0字节的workbook和worksheet,以及2个res文档)
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

            //建立workbook文档,设定模式的worksheet (sheet的3个属性必须填写,特别的是name是这里设定,有点不符合常见的抽象思维)
            Workbook workbook = new Workbook();
            Sheets sheets = new Sheets();
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = firstSheetName
            };
            sheets.Append(sheet);
            workbook.AppendChild<Sheets>(sheets);
            workbookpart.Workbook = workbook;

            //建立默认的worksheet文档(可以先workbook,后worksheet)
            SheetData sheetData = new SheetData();
            Worksheet worksheet = new Worksheet();
            worksheet.Append(sheetData);
            worksheetPart.Worksheet = worksheet;//给默认的worksheet赋值,否则0字节.

            //建立样式文件
            createDeafultStyle(workbookpart, out defaultCellStyle);
            spreadsheetDocument.Save();


            return spreadsheetDocument;
        }

        private static SpreadsheetDocument openFile(string filepath, out DefaultCellStyle defaultCellStyle)
        {
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, true);
            SetExcelPivotTableCacheAutoReflesh(spreadsheetDocument);
            createDeafultStyle(spreadsheetDocument.WorkbookPart, out defaultCellStyle);
            return spreadsheetDocument;
        }

        #endregion

        #region row
        private static bool CreateOrUpdateRowAt(SheetData sheetData, DataRow dataRow, uint rowNo, uint columnNo, DefaultCellStyle defaultCellStyle, Dictionary<uint, uint> eachColumnStyle)
        {
            if (rowNo > 0 && columnNo > 0 && defaultCellStyle != null && sheetData != null)
            {
                Row newRow = CreateRow(rowNo, columnNo, dataRow, defaultCellStyle, eachColumnStyle);
                CreateOrUpdateRowAt(sheetData, newRow, newRow.RowIndex);
            }
            return true;
        }

        private static bool CreateOrUpdateRowAt(SheetData sheetData, Row newRow, uint rowNo)
        {
            bool res = false;
            //create row and cell. append or insert.
            if (sheetData != null && newRow != null)
            {
                //是否存在row,1.存在,删除.2.现在是否存在大于row,存在,在前插入.不存在.直接附加.
                var rows = sheetData.Elements<Row>().Where(x => x.RowIndex == rowNo);
                if (rows.Count() > 0)
                {
                    sheetData.RemoveChild(rows.First());
                }

                var biggerrows = sheetData.Elements<Row>().Where(x => x.RowIndex > rowNo);
                if (biggerrows.Count() <= 0)
                {
                    sheetData.Append(newRow);//todo:装载数据7/10的时间耗费在这里。需要优化！如果是大数据插入，应该创建大量row，再使用一次append或其他插入函数。
                }
                else
                {
                    sheetData.InsertBefore(newRow, biggerrows.First());
                }
                res = true;
            }
            return res;
        }

        //Dictionary<int, uint> 表格列的索引(from zero)和样式编号
        private static Row CreateRow(uint rowNo, uint columnNo, DataRow dataRow, DefaultCellStyle defaultCellStyle, Dictionary<uint, uint> eachCellStyle = null)
        {
            Row newRow = new Row();
            newRow.RowIndex = rowNo;

            if (dataRow != null)
            {
                for (uint i = 0; i < dataRow.Table.Columns.Count; i++)
                {
                    Cell newCell = null;
                    string cellref = GetCellReference(rowNo, columnNo + i);
                    Type cellType = dataRow.Table.Columns[(int)i].DataType;
                    object cellValue = dataRow[(int)i];
                    if (eachCellStyle != null && eachCellStyle.Keys.Contains(columnNo + i))
                    {
                        newCell = createCell(cellref, cellType, cellValue, defaultCellStyle, eachCellStyle[columnNo + i]);
                    }
                    else
                    {
                        newCell = createCell(cellref, cellType, cellValue, defaultCellStyle);
                    }
                    if (newCell != null)
                    {
                        newRow.Append(newCell);
                    }
                }
            }

            return newRow;
        }

        private static IEnumerable<Row> GetRows(SheetData sheetdata, uint startRowNo, uint endRowNo)
        {
            IEnumerable<Row> res = null;
            if (sheetdata != null && endRowNo >= startRowNo)
            {
                var rows = sheetdata.Elements<Row>().Where(x => x.RowIndex >= startRowNo && x.RowIndex <= endRowNo);
                res = rows;
            }
            return res;
        }

        private static List<string> GetRowsXml(SheetData sheetdata, uint startRowNo, uint endRowNo)
        {
            List<string> rowsXml = new List<string>();
            IEnumerable<Row> rows = GetRows(sheetdata, startRowNo, endRowNo);
            if (rows != null)
            {
                foreach (Row row in rows)
                {
                    rowsXml.Add(row.OuterXml);
                }
            }
            return rowsXml;
        }

        private static void ChangeRowNoAndCellReference(Row row, uint newRowNo)//行变化行号后，需要修改row和cell的行号。
        {
            if (row != null)
            {
                uint preIndex = row.RowIndex;
                row.RowIndex = newRowNo;
                var allcells = row.Elements<Cell>();
                foreach (Cell cell in allcells)
                {
                    cell.CellReference = cell.CellReference.Value.Replace(preIndex.ToString(), row.RowIndex.ToString());
                }
            }
        }

        private static void DeleteRows(IEnumerable<Row> rows)
        {
            if (rows != null)
            {
                foreach (Row r in rows.ToList())//删除不能用可碟带类型,因为需要当前来movenext.先换成集合把.
                {
                    r.Remove();
                }
            }
        }

        #endregion

        #region cell
        private static Cell createCell(string reference, Type type, object value, DefaultCellStyle defaultCellStyle, uint customStyle = 0)
        {
            if (reference == null || type == null || defaultCellStyle == null)
            {
                return null;
            }
            Cell theCell = new Cell();
            theCell.CellReference = reference;

            CellValues theCellValue = GetValueType(type);
            theCell.DataType = new EnumValue<CellValues>(theCellValue);

            if (theCellValue == CellValues.Date)
            {
                if (value.ToString() != "")
                {
                    theCell.CellValue = new CellValue(((DateTime)value));
                }
                else
                {
                    theCell.CellValue = new CellValue("");
                }
                if (customStyle == 0)
                {
                    theCell.StyleIndex = defaultCellStyle.dateTimeIndex;
                }
                else
                {
                    theCell.StyleIndex = customStyle;
                }
            }
            else
            {
                theCell.CellValue = new CellValue(value.ToString());
                if (customStyle == 0)
                {
                    theCell.StyleIndex = defaultCellStyle.normalIndex;
                }
                else
                {
                    theCell.StyleIndex = customStyle;
                }
            }
            return theCell;
        }
        private static bool SetOrUpdateCellValue(SheetData sheetData, uint rowNumber, uint columnNumber, Type datatype, object value, DefaultCellStyle defaultCellStyle, uint customStyle = 0)
        {
            if (sheetData != null && value != null && datatype != null)
            {
                //创建cell.
                //0.不存在row,建立row,插入cell.
                //0.row存在1.删除cell.2.是否有比这个cell更大的cell,有插入大cell之前.否则直接附加在row后面.
                string cellRefrence = GetCellReference(rowNumber, columnNumber);

                IEnumerable<Row> equalOrbiggerRows = sheetData.Elements<Row>().Where(x => x.RowIndex >= rowNumber);
                Row equalRow = null, biggerRow = null;
                if (equalOrbiggerRows != null && equalOrbiggerRows.Count() > 0 && equalOrbiggerRows.First().RowIndex == rowNumber)
                {
                    equalRow = equalOrbiggerRows.First();
                }
                else if (equalOrbiggerRows != null && equalOrbiggerRows.Count() > 0 && equalOrbiggerRows.First().RowIndex > rowNumber)
                {
                    biggerRow = equalOrbiggerRows.First();
                }

                if (equalRow != null)//存在row.  1.是否存在cell,存在跟新,不存在建立新cell.2.是否有比这个cell更大的cell,有插入大cell之前.否则直接附加在Row后面.
                {
                    IEnumerable<Cell> equalOrbiggerCells = equalRow.Elements<Cell>().Where(x => x.CellReference >= new StringValue(cellRefrence));

                    Cell equalCell = null;
                    Cell biggerCell = null;
                    if (equalOrbiggerCells != null && equalOrbiggerCells.Count() > 0 && equalOrbiggerCells.First().CellReference == cellRefrence)
                    {
                        equalCell = equalOrbiggerCells.First();
                    }
                    else if (equalOrbiggerCells != null && equalOrbiggerCells.Count() > 0 && equalOrbiggerCells.First().CellReference > new StringValue(cellRefrence))
                    {
                        biggerCell = equalOrbiggerCells.First();
                    }

                    Cell newCell = createCell(cellRefrence, datatype, value, defaultCellStyle, customStyle);
                    if (equalCell != null)
                    {
                        equalOrbiggerRows.First().ReplaceChild(newCell, equalCell);
                    }
                    else
                    {
                        if (biggerCell != null)
                        {
                            equalOrbiggerRows.First().InsertBefore(newCell, biggerCell);
                        }
                        else
                        {
                            equalOrbiggerRows.First().Append(newCell);
                        }
                    }
                }
                else//不存在.新建row and cell.
                {
                    Row newrow = new Row();
                    newrow.RowIndex = rowNumber;

                    Cell theCell = IncOpenExcel.createCell(cellRefrence, datatype, value, defaultCellStyle, customStyle);
                    if (theCell != null)
                    {
                        newrow.Append(theCell);
                    }

                    if (biggerRow != null)
                    {
                        sheetData.InsertBefore(newrow, equalOrbiggerRows.First());//插入的行不是最大的,插到比它大的前面.
                    }
                    else
                    {
                        sheetData.Append(newrow); ;//插入的行是最大的,直接附加到最后
                    }
                }
            }
            return true;
        }
        public static string GetCellRealString(SpreadsheetDocument document, Cell cell)
        {
            string res = null;
            if (cell != null)
            {
                if (cell.DataType != null)
                {
                    CellValues cellType = cell.DataType;
                    if (cellType == CellValues.SharedString)
                    {
                        res = getShareString(document, int.Parse(cell.CellValue.Text));
                    }
                    else
                    {
                        res = cell.CellValue.Text;
                    }
                }
                else
                {
                    res = cell.CellValue == null ? "" : cell.CellValue.Text;
                }
            }
            return res;
        }
        #endregion

        #region other


        private static void SetExcelPivotTableCacheAutoReflesh(SpreadsheetDocument document)
        {
            if (document != null)
            {
                WorkbookPart wbPart = document.WorkbookPart;
                var pivottableCashes = wbPart.PivotTableCacheDefinitionParts;
                foreach (PivotTableCacheDefinitionPart pivottablecachePart in pivottableCashes)
                {
                    pivottablecachePart.PivotCacheDefinition.RefreshOnLoad = true;
                }
            }
        }

        public static string GetCellReference(uint rowIndex, uint colIndex)
        {
            UInt32 dividend = colIndex;
            string columnName = String.Empty;
            UInt32 modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (UInt32)((dividend - modifier) / 26);
            }

            return columnName + rowIndex.ToString();
        }
        private static CellValues GetValueType(Type type)
        {
            List<Type> number = new List<Type> { typeof(Double), typeof(Decimal), typeof(Int32), typeof(int), typeof(Int16), typeof(Int64) };
            List<Type> date = new List<Type> { typeof(DateTime) };

            if (number.Contains(type))
            {
                return CellValues.Number;
            }
            else if (date.Contains(type))
            {
                return CellValues.Date;
            }
            else
            {
                return CellValues.String;
            }
        }
        public static DataTable GetColumnsNames(DataTable dataTable)
        {
            DataTable databable_books = new DataTable("ColumnsNames");
            DataRow newRow = databable_books.NewRow();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                databable_books.Columns.Add("column" + i, Type.GetType("System.String"));

                newRow["column" + i] = dataTable.Columns[i].ColumnName;
            }
            databable_books.Rows.Add(newRow);
            return databable_books;
        }
        private static string getShareString(SpreadsheetDocument documet, int index)
        {
            string res = null;
            if (documet != null && documet.WorkbookPart.SharedStringTablePart != null && documet.WorkbookPart.SharedStringTablePart.SharedStringTable != null)
            {
                IEnumerable<SharedStringItem> items = documet.WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>();
                if (items.Count() > index)
                {
                    res = items.ElementAt(index).Text.InnerText;
                }
            }
            return res;
        }
        #endregion

        #region font ,cellformat
        //建立一个字体
        private static UInt32Value createFont(Stylesheet styleSheet, string fontName, Nullable<double> fontSize, bool isBold, System.Drawing.Color foreColor)
        {
            if (styleSheet.Fonts.Count == null)
            {
                styleSheet.Fonts.Count = (UInt32Value)0;
            }
            Font font = new Font();
            if (!string.IsNullOrEmpty(fontName))
            {
                FontName name = new FontName()
                {
                    Val = fontName
                };
                font.Append(name);
            }

            if (fontSize.HasValue)
            {
                FontSize size = new FontSize()
                {
                    Val = fontSize.Value
                };
                font.Append(size);
            }

            if (isBold == true)
            {
                Bold bold = new Bold();
                font.Append(bold);
            }

            if (foreColor != null)
            {
                Color color = new Color()
                {
                    Rgb = new HexBinaryValue()
                    {
                        Value =
                            System.Drawing.ColorTranslator.ToHtml(
                                System.Drawing.Color.FromArgb(
                                    foreColor.A,
                                    foreColor.R,
                                    foreColor.G,
                                    foreColor.B)).Replace("#", "")
                    }
                };
                font.Append(color);
            }
            styleSheet.Fonts.Append(font);
            UInt32Value result = styleSheet.Fonts.Count;
            styleSheet.Fonts.Count++;
            return result;
        }
        //创建一个单元格样式
        private static UInt32Value createCellFormat(Stylesheet styleSheet, UInt32Value borderid, UInt32Value fontIndex, UInt32Value fillIndex, UInt32Value numberFormatId)
        {
            if (styleSheet.CellFormats.Count == null)
            {
                styleSheet.CellFormats.Count = (UInt32Value)0;
            }
            CellFormat cellFormat = new CellFormat();
            cellFormat.BorderId = 0;
            if (fontIndex != null)
            {
                cellFormat.ApplyFont = true;
                cellFormat.FontId = fontIndex;
            }
            if (borderid != null)
            {
                cellFormat.ApplyBorder = true;
                cellFormat.BorderId = borderid;
            }
            if (fillIndex != null)
            {
                cellFormat.FillId = fillIndex;
                cellFormat.ApplyFill = true;
            }
            if (numberFormatId != null)
            {
                cellFormat.NumberFormatId = numberFormatId;
                cellFormat.ApplyNumberFormat = true;
            }

            styleSheet.CellFormats.Append(cellFormat);
            UInt32Value result = styleSheet.CellFormats.Count;
            styleSheet.CellFormats.Count++;
            return result;
        }

        private static UInt32Value createEmptyBorder(Stylesheet styleSheet)
        {
            styleSheet.Borders.Count = styleSheet.Borders.Count == null ? 0 : styleSheet.Borders.Count;

            Border tempBorder = new Border(new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder());

            styleSheet.Borders.Append(tempBorder);
            UInt32Value res = styleSheet.Borders.Count;
            styleSheet.Borders.Count++;
            return res;
        }


        private static UInt32Value create4LinesBorder(Stylesheet styleSheet)
        {
            styleSheet.Borders.Count = styleSheet.Borders.Count == null ? 0 : styleSheet.Borders.Count;

            RightBorder rightBorder = new RightBorder();
            rightBorder.Style = BorderStyleValues.Thin;
            TopBorder topBorder = new TopBorder();
            topBorder.Style = BorderStyleValues.Thin;
            BottomBorder bottomBorder = new BottomBorder();
            bottomBorder.Style = BorderStyleValues.Thin;
            LeftBorder leftBorder = new LeftBorder();
            leftBorder.Style = BorderStyleValues.Thin;
            Border tempBorder = new Border();
            tempBorder.LeftBorder = leftBorder;
            tempBorder.RightBorder = rightBorder;
            tempBorder.TopBorder = topBorder;
            tempBorder.BottomBorder = bottomBorder;

            styleSheet.Borders.Append(tempBorder);
            UInt32Value res = styleSheet.Borders.Count;
            styleSheet.Borders.Count++;
            return res;
        }


        //建立一个最小样式表.
        private static void createDeafultStyle(WorkbookPart workbookpart, out DefaultCellStyle cellStyle)
        {
            cellStyle = new DefaultCellStyle(0, 0, 0, 0, 0, 0, 0, 0);
            if (workbookpart != null)
            {
                //1.建立必要的文件和其根节点.
                if (workbookpart.WorkbookStylesPart == null)
                {
                    workbookpart.AddNewPart<WorkbookStylesPart>();
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet = new Stylesheet();
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet.Fonts == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet.Fonts = new Fonts();
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet.Fills == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet.Fills = new Fills(new Fill(new PatternFill() { PatternType = PatternValues.None }), new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }));
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet.Borders == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet.Borders = new Borders();
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet.CellFormats == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet.CellFormats = new CellFormats();
                }
                if (workbookpart.WorkbookStylesPart.Stylesheet.NumberingFormats == null)
                {
                    workbookpart.WorkbookStylesPart.Stylesheet.NumberingFormats = new NumberingFormats();
                }

                //自定义字体
                UInt32 defaultFont = createFont(workbookpart.WorkbookStylesPart.Stylesheet, "Microsoft YaHei", (double)11, false, System.Drawing.Color.Black);
                UInt32 BoldFont = createFont(workbookpart.WorkbookStylesPart.Stylesheet, "Microsoft YaHei", (double)11, true, System.Drawing.Color.Black);
                uint borderEmpty = createEmptyBorder(workbookpart.WorkbookStylesPart.Stylesheet);
                uint border4Line = create4LinesBorder(workbookpart.WorkbookStylesPart.Stylesheet);
                //自定义数字格式,时间格式
                UInt32 dateDeafult = 240;
                var numberFormatDate_index = new NumberingFormat();
                numberFormatDate_index.FormatCode = new StringValue("yyyy-mm-dd");
                numberFormatDate_index.NumberFormatId = dateDeafult;//随意定义100~200之间.
                workbookpart.WorkbookStylesPart.Stylesheet.NumberingFormats.InsertAt(numberFormatDate_index, workbookpart.WorkbookStylesPart.Stylesheet.NumberingFormats.Count());
                //自定义最终给用户的单元格式.
                cellStyle.normalIndex = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, null, defaultFont, null, null);
                cellStyle.dateTimeIndex = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, null, defaultFont, null, dateDeafult);

                cellStyle.blackIndex = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, null, BoldFont, null, null);
                cellStyle.blackdateTimeIndex = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, null, BoldFont, null, dateDeafult);

                cellStyle.normalIndex_border = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, border4Line, defaultFont, null, null);
                cellStyle.dateTimeIndex_border = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, border4Line, defaultFont, null, dateDeafult);

                cellStyle.blackIndex_border = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, border4Line, BoldFont, null, null);
                cellStyle.blackdateTimeIndex_border = createCellFormat(workbookpart.WorkbookStylesPart.Stylesheet, border4Line, BoldFont, null, dateDeafult);

                workbookpart.WorkbookStylesPart.Stylesheet.Save();
            }
        }
        #endregion

        #endregion

        #region innerclass
        public class DefaultCellStyle
        {
            public UInt32 normalIndex { get; set; }
            public UInt32 dateTimeIndex { get; set; }

            public uint normalIndex_border;
            public uint dateTimeIndex_border;

            public UInt32 blackIndex { get; set; }
            public UInt32 blackdateTimeIndex { get; set; }

            public uint blackIndex_border;
            public UInt32 blackdateTimeIndex_border { get; set; }

            public DefaultCellStyle(uint normalIndex, uint dateTimeIndex, uint normalIndex_border, uint dateTimeIndex_border, uint blackIndex, uint blackdateTimeIndex, uint blackIndex_border, uint blackdateTimeIndex_border)
            {
                this.normalIndex = normalIndex;
                this.dateTimeIndex = dateTimeIndex;
                this.normalIndex_border = normalIndex_border;
                this.dateTimeIndex_border = dateTimeIndex_border;
                this.blackIndex = blackIndex;
                this.blackdateTimeIndex = blackdateTimeIndex;
                this.blackIndex_border = blackIndex_border;
                this.blackdateTimeIndex_border = blackdateTimeIndex_border;
            }




        }
        #endregion
    }
}