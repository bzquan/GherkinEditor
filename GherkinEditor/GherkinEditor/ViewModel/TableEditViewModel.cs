using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid;
using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Gherkin.Model;
using static Gherkin.Model.GherkinFormatUtil;
using System.Windows;
using unvell.ReoGrid.Actions;
using unvell.ReoGrid.Formula;
using Gherkin.Util.Geometric;

namespace Gherkin.ViewModel
{
    public class TableEditViewModel : NotifyPropertyChangedBase
    {
        private IAppSettings m_Appsetting;
        private ReoGridControl m_TableGridControl;

        public TableEditViewModel(IAppSettings appsetting)
        {
            m_Appsetting = appsetting;
            EventAggregator<StartEditTableRequestArg>.Instance.Event += OnStartEditTableRequested;
            EventAggregator<ReplaceTableFromGridArg>.Instance.Event += OnReplaceTableFromGrid;
            EventAggregator<PasteTableFromGridArg>.Instance.Event += OnPasteTableFromGrid;
        }

        public ReoGridControl TableGridControl
        {
            get { return m_TableGridControl; }
            set
            {
                m_TableGridControl = value;
                InitializeSheet(m_TableGridControl.Worksheets[0], "Work Area");
                RegisterFormulas();
            }
        }

        #region ReoGrid edit

        public void CopyCells()
        {
            try
            {
                this.CurrentWorksheet?.Copy();
            }
            catch (RangeIntersectionException ex)
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show("Copy failed", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CutCells()
        {
            try
            {
                this.CurrentWorksheet.Cut();
            }
            catch (RangeIntersectionException ex)
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show("Cut failed", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void PasteCells()
        {
            try
            {
                this.CurrentWorksheet.Paste();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertCopiedColumns()
        {
            try
            {
                this.m_TableGridControl.DoAction(new InsertColumnsAction(CurrentSelectionRange.Col, CurrentSelectionRange.Cols));
                this.CurrentWorksheet.Paste();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertCopiedRows()
        {
            try
            {
                this.m_TableGridControl.DoAction(new InsertRowsAction(CurrentSelectionRange.Row, CurrentSelectionRange.Rows));
                this.CurrentWorksheet.Paste();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InsertNewColumn()
        {
            if (this.CurrentSelectionRange.Cols >= 1)
            {
                this.m_TableGridControl.DoAction(new InsertColumnsAction(CurrentSelectionRange.Col, CurrentSelectionRange.Cols));
            }
        }

        public void InsertNewRow()
        {
            if (this.CurrentSelectionRange.Rows >= 1)
            {
                this.m_TableGridControl.DoAction(new InsertRowsAction(CurrentSelectionRange.Row, CurrentSelectionRange.Rows));
            }
        }

        public void DeleteColumns()
        {
            if (this.CurrentSelectionRange.Cols >= 1)
            {
                this.m_TableGridControl.DoAction(new RemoveColumnsAction(CurrentSelectionRange.Col, CurrentSelectionRange.Cols));
            }
        }

        public void DeleteRows()
        {
            if (this.CurrentSelectionRange.Rows >= 1)
            {
                this.m_TableGridControl.DoAction(new RemoveRowsAction(CurrentSelectionRange.Row, CurrentSelectionRange.Rows));
            }
        }

        public void ConvertLonLat2XY()
        {
            List<GPoint> XY = CurveViewCache.LonLat2XY(ExtractCoordinates());
            InsertXY(XY, "X", "Y");
        }

        public bool HasEnoughPoints(int minRows)
        {
            if (this.CurrentSelectionRange.Cols != 2) return false;

            var col1 = this.CurrentSelectionRange.Col;
            var col2 = this.CurrentSelectionRange.EndCol;
            for (int row = 1; row <= minRows; row++)
            {
                if (!IsCellDouble(row, col1) || !IsCellDouble(row, col2)) return false;
            }
            return true;
        }

        public void CalcCurvatureFromLonLat(string curvatureCount)
        {
            List<GPoint> XY = CurveViewCache.LonLat2XY(ExtractCoordinates());
            int expectedCurvatureCount = Util.StringUtil.IsDigitsOnly(curvatureCount) ? int.Parse(curvatureCount) : XY.Count;
            InsertCurvature(XY, expectedCurvatureCount);
        }

        public void CalcCurvatureFromXY(string curvatureCount)
        {
            List<GPoint> XY = ExtractCoordinates();
            int expectedCurvatureCount = Util.StringUtil.IsDigitsOnly(curvatureCount) ? int.Parse(curvatureCount) : XY.Count;
            InsertCurvature(XY, expectedCurvatureCount);
        }

        public void SimplifyPoints(string expectedPointsNum)
        {
            List<GPoint> XY = ExtractCoordinates();
            int expectedPointsCount = Util.StringUtil.IsDigitsOnly(expectedPointsNum) ? int.Parse(expectedPointsNum) : 500;
            List<GPoint> simpfiledXY;
            if (m_Appsetting.CurveManeuverParameter.ThinoutByDouglasPeuckerN)
                simpfiledXY = PolylineSimplication.SimplifyByDouglasPeuckerN(XY.ToArray(), expectedPointsCount);
            else
                simpfiledXY = PolylineSimplication.SimplifyPointsN(XY.ToArray(), maxPoints: expectedPointsCount);

            var col1 = this.CurrentSelectionRange.Col;
            var col2 = this.CurrentSelectionRange.EndCol;
            string newCol1Name = "new" + CurrentWorksheet[0, col1];
            string newCol2Name = "new" + CurrentWorksheet[0, col2];
            InsertXY(simpfiledXY, newCol1Name, newCol2Name);
        }

        private bool IsCellDouble(int row, int col) => CurrentWorksheet[row, col] is double;

        private Worksheet CurrentWorksheet => this.m_TableGridControl.CurrentWorksheet;

        private RangePosition CurrentSelectionRange
        {
            get { return this.m_TableGridControl.CurrentWorksheet.SelectionRange; }
            set { this.m_TableGridControl.CurrentWorksheet.SelectionRange = value; }
        }
        #endregion

        private void RegisterFormulas()
        {
            // longitude to X on plane coordinates
            FormulaExtension.CustomFunctions["LON"] = (cell, args) => {
                if ((args.Length != 3) || !(args[0] is double) || !(args[1] is double) || !(args[2] is double))
                    return "Arguments should be longitude, latitude and start longitude";

                return WGS8GeoCoordinate.Lon2X((double)args[0], (double)args[1], (double)args[2]);
            };

            // latitude to Y on plane coordinates
            FormulaExtension.CustomFunctions["LAT"] = (cell, args) => {
                if ((args.Length != 2) || !(args[0] is double) || !(args[1] is double))
                    return "Arguments should be latitude and start latitude";

                return WGS8GeoCoordinate.Lat2Y((double)args[0], (double)args[1]);
            };
        }

        private void InitializeSheet(Worksheet sheet, string name)
        {
            sheet.Name = name;
            sheet.RowCount = 300;
            sheet.ColumnCount = 52;
        }

        private void OnStartEditTableRequested(object sender, StartEditTableRequestArg arg)
        {
            var sheet = CreateWorkSheet();
            sheet.FreezeToCell(1, 0);
            TableGridControl.MoveWorksheet(sheet, 0);
            TableGridControl.CurrentWorksheet = sheet;

            int maxColumnNum;
            SetGherkinTable(sheet, arg.TextEditor, out maxColumnNum);
            AdjustColumnWidth(sheet, maxColumnNum);
        }

        private void OnReplaceTableFromGrid(object sender, ReplaceTableFromGridArg arg)
        {
            string table_text = TableExtractorFromGrid();
            if (string.IsNullOrEmpty(table_text)) return;

            DocumentLine beginLine;
            DocumentLine endLine;
            TableExtractorFromTextEditor.ExtractTableRange(arg.TextEditor.TextArea, out beginLine, out endLine);
            if ((beginLine != null) && (endLine != null))
            {
                int startOffset = beginLine.Offset;
                int length = endLine.Offset + endLine.TotalLength - startOffset - 1;
                arg.TextEditor.TextArea.Document.Replace(startOffset, length, table_text);
            }
        }

        private void OnPasteTableFromGrid(object sender, PasteTableFromGridArg arg)
        {
            string table_text = TableExtractorFromGrid();
            if (string.IsNullOrEmpty(table_text)) return;

            var textArea = arg.TextEditor.TextArea;
            int offset = textArea.Caret.Offset;
            var line = textArea.Document.GetLineByOffset(offset);
            textArea.Document.Replace(line.Offset, line.Length, table_text);
        }

        private string TableExtractorFromGrid()
        {
            if (TableGridControl.CurrentWorksheet == null) return null;

            var extractor = new TableExtractorFromGrid(TableGridControl.CurrentWorksheet);
            return extractor.GetTableText();
        }

        private Worksheet CreateWorkSheet()
        {
            Worksheet sheet = m_TableGridControl.NewWorksheet();
            string name = NewWorkSheetName();
            InitializeSheet(sheet, name);

            return sheet;
        }

        private string NewWorkSheetName()
        {
            string GherkinTableName = "Table";
            int GherkinTableNameLenth = GherkinTableName.Length;
            int maxWorkSheetNo = 0;
            foreach (var sheet in TableGridControl.Worksheets)
            {
                string name = sheet.Name;
                if ((name.Length > GherkinTableNameLenth) &&
                    name.StartsWith(GherkinTableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    string numText = name.Substring(GherkinTableNameLenth).Trim();
                    if (Util.StringUtil.IsDigitsOnly(numText))
                    {
                        int num = int.Parse(numText);
                        maxWorkSheetNo = Math.Max(num, maxWorkSheetNo);
                    }
                }
            }

            return GherkinTableName + (maxWorkSheetNo + 1);
        }

        private void SetGherkinTable(Worksheet sheet, TextEditor editor, out int maxColumnNum)
        {
            maxColumnNum = 0;

            TableExtractorFromTextEditor manipulator = new TableExtractorFromTextEditor(editor);
            List<List<string>> table = manipulator.GetCurrentTable();
            if (sheet.RowCount < table.Count())
            {
                sheet.RowCount = table.Count() + 1;
            }
            SetCellValues(sheet, table, out maxColumnNum);
        }

        private void SetCellValues(Worksheet sheet, List<List<string>> table, out int maxColumnNum)
        {
            maxColumnNum = 0;
            for (int i = 0; i < table.Count(); i++)
            {
                var row = table[i];
                for (int k = 0; k < row.Count; k++)
                {
                    sheet[i, k] = row[k];
                }

                if (row.Count > maxColumnNum)
                    maxColumnNum = row.Count;
            }
        }

        private void AdjustColumnWidth(Worksheet sheet, int maxColumnNum)
        {
            for (int i = 0; i < maxColumnNum; i++)
                sheet.AutoFitColumnWidth(i);
        }

        private List<GPoint> ExtractCoordinates()
        {
            var col1 = this.CurrentSelectionRange.Col;
            var col2 = this.CurrentSelectionRange.EndCol;

            List<GPoint> points = new List<GPoint>();
            for (int row = 1; row < CurrentWorksheet.RowCount; row++)
            {
                if (IsCellDouble(row, col1) && IsCellDouble(row, col2))
                {
                    double x = (double)CurrentWorksheet[row, col1];
                    double y = (double)CurrentWorksheet[row, col2];
                    points.Add(new GPoint(x, y));
                }
                else
                    break;
            }

            return points;
        }

        private void InsertXY(List<GPoint> XY, string col1Name, string col2Name)
        {
            InsertNewColumn();
            var col1 = this.CurrentSelectionRange.Col;
            var col2 = this.CurrentSelectionRange.EndCol;

            CurrentWorksheet[0, col1] = col1Name;
            CurrentWorksheet[0, col2] = col2Name;
            for (int row = 0; row < XY.Count; row++)
            {
                CurrentWorksheet[row + 1, col1] = XY[row].X;
                CurrentWorksheet[row + 1, col2] = XY[row].Y;
                if (XY[row].IsNoise)
                {
                    CurrentWorksheet.SetRangeStyles(row + 1, col2, 1, 1, new WorksheetRangeStyle
                    {
                        Flag = PlainStyleFlag.BackColor,
                        BackColor = unvell.ReoGrid.Graphics.SolidColor.Red
                    });
                }
            }

            CurrentWorksheet.AutoFitColumnWidth(col1);
            CurrentWorksheet.AutoFitColumnWidth(col2);
        }

        private void InsertCurvature(List<GPoint> XY, int expectedCurvatureCount)
        {
            int additionalNum = 3;
            List<GPoint> curvaturePoints = Util.NURBS.NURBSCurve.CalcCurvature(XY, expectedCurvatureCount + additionalNum);

            string curvatureTitle;
            if ((m_Appsetting.CurveManeuverParameter.Unit == CurvatureUnit.Meter))
                curvatureTitle = "Curvature(1/m)";
            else
                curvatureTitle = "Curvature(1/cm)";

            InsertXY(curvaturePoints, "Step", curvatureTitle);
        }
    }
}
