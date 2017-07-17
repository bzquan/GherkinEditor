using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unvell.ReoGrid;
using unvell.ReoGrid.Actions;
using Gherkin.ViewModel;
using Gherkin.Util;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for ReoGridWrapper.xaml
    /// https://reogrid.net/
    /// </summary>
    public partial class ReoGridWrapper : UserControl
    {
        private TableEditViewModel m_TableEditViewModel;

        private System.Windows.Forms.ToolStripMenuItem m_LonLat2XYColToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem m_LonLat2CurvatureColToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem m_XY2CurvatureColToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem m_PolylinSimplificationColToolStripMenuItem;

        private bool ShowADASMenus => ConfigReader.GetValue<bool>("showADASMenusForTableEditor", false);

        public ReoGridWrapper()
        {
            InitializeComponent();
        }

        public void SetTableEditViewModel(TableEditViewModel tableEditViewModel)
        {
            m_TableEditViewModel = tableEditViewModel;
            m_TableEditViewModel.TableGridControl = reoGridControl;

            SetColumnContextMenu();
            SetRowContextMenu();
            SetCellContextMenu();
        }

        private void SetColumnContextMenu()
        {
            var copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CopyMenuItemText };
            copyToolStripMenuItem.Image = ImageWpfToGDI("Copy.png");
            copyToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CopyCells();

            var cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CutMenuItemText };
            cutToolStripMenuItem.Image = ImageWpfToGDI("Cut.png");
            cutToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CutCells();

            var pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = PasteMenuItemText };
            pasteToolStripMenuItem.Image = ImageWpfToGDI("Paste.png");
            pasteToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.PasteCells();

            var insertCopiedColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_InsertCopiedColumns };
            insertCopiedColToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.InsertCopiedColumns();

            var insertNewColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_InsertColumn };
            insertNewColToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.InsertNewColumn();

            var deleteColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_DeleteColumn };
            deleteColToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.DeleteColumns();

            var colContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            colContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                copyToolStripMenuItem,
                cutToolStripMenuItem,
                pasteToolStripMenuItem,
                insertCopiedColToolStripMenuItem,
                insertNewColToolStripMenuItem,
                deleteColToolStripMenuItem,
            });

            if (ShowADASMenus)
            {
                m_LonLat2XYColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_LonLat2XY };
                m_LonLat2XYColToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.ConvertLonLat2XY();

                m_LonLat2CurvatureColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_CalcCurvatureFromLonLat };
                m_LonLat2CurvatureColToolStripMenuItem.Click += LonLat2CurvatureColToolStripMenuItem_Click;

                m_XY2CurvatureColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_CalcCurvatureFromXY };
                m_XY2CurvatureColToolStripMenuItem.Click += XY2CurvatureColToolStripMenuItem_Click;

                m_PolylinSimplificationColToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_SimplifyPoints };
                m_PolylinSimplificationColToolStripMenuItem.Click += PolylinSimplificationColToolStripMenuItem_Click;

                colContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                m_LonLat2XYColToolStripMenuItem,
                m_LonLat2CurvatureColToolStripMenuItem,
                m_XY2CurvatureColToolStripMenuItem,
                m_PolylinSimplificationColToolStripMenuItem
                });
            }

            colContextMenuStrip.Opening += OnOpenningColumnContextMenuStrip;
            this.reoGridControl.ColumnHeaderContextMenuStrip = colContextMenuStrip;
        }

        private void XY2CurvatureColToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var input = GetExpectedPointsCount(ExpectedPointsNumSelections);
            m_TableEditViewModel.CalcCurvatureFromXY(input);
        }

        private void PolylinSimplificationColToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> expectedPointsNumSelections = new List<string>() { "500" };
            var expectedPointsNum = GetExpectedPointsCount(expectedPointsNumSelections);
            m_TableEditViewModel.SimplifyPoints(expectedPointsNum);
        }

        private List<string> ExpectedPointsNumSelections => new List<string>() { Properties.Resources.InputCurvatureNum_SameAsInput, "100" };
        private void LonLat2CurvatureColToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var input = GetExpectedPointsCount(ExpectedPointsNumSelections);
            m_TableEditViewModel.CalcCurvatureFromLonLat(input);
        }

        private string GetExpectedPointsCount(List<string> expectedPointsNumSelections)
        {
            var input = new InputExpectedPointsNum(expectedPointsNumSelections);

            input.Left = this.reoGridControl.ColumnHeaderContextMenuStrip.Left;
            input.Top = this.reoGridControl.ColumnHeaderContextMenuStrip.Top + 20;
            input.ShowDialog();
            return input.InputValue;
        }

        private void OnOpenningColumnContextMenuStrip(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ShowADASMenus)
            {
                m_LonLat2XYColToolStripMenuItem.Enabled = m_TableEditViewModel.HasEnoughPoints(minRows: 2);

                bool hasMoreThan4Points = m_TableEditViewModel.HasEnoughPoints(minRows: 4);
                m_LonLat2CurvatureColToolStripMenuItem.Enabled = hasMoreThan4Points;
                m_XY2CurvatureColToolStripMenuItem.Enabled = hasMoreThan4Points;
                m_PolylinSimplificationColToolStripMenuItem.Enabled = hasMoreThan4Points;
            }
        }

        private void SetRowContextMenu()
        {
            var copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CopyMenuItemText };
            copyToolStripMenuItem.Image = ImageWpfToGDI("Copy.png");
            copyToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CopyCells();

            var cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CutMenuItemText };
            cutToolStripMenuItem.Image = ImageWpfToGDI("Cut.png");
            cutToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CutCells();

            var pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = PasteMenuItemText };
            pasteToolStripMenuItem.Image = ImageWpfToGDI("Paste.png");
            pasteToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.PasteCells();

            var insertCopiedRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_InsertCopiedRows };
            insertCopiedRowToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.InsertCopiedRows();

            var insertNewRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_InsertRow };
            insertNewRowToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.InsertNewRow();

            var deleteRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = Properties.Resources.MenuReoGrid_DeleteRow };
            deleteRowsToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.DeleteRows();

            var rowContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            rowContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                copyToolStripMenuItem,
                cutToolStripMenuItem,
                pasteToolStripMenuItem,
                insertCopiedRowToolStripMenuItem,
                insertNewRowToolStripMenuItem,
                deleteRowsToolStripMenuItem });
            this.reoGridControl.RowHeaderContextMenuStrip = rowContextMenuStrip;
        }

        private void SetCellContextMenu()
        {
            var copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CopyMenuItemText };
            copyToolStripMenuItem.Image = ImageWpfToGDI("Copy.png");
            copyToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CopyCells();

            var cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = CutMenuItemText };
            cutToolStripMenuItem.Image = ImageWpfToGDI("Cut.png");
            cutToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.CutCells();

            var pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text = PasteMenuItemText };
            pasteToolStripMenuItem.Image = ImageWpfToGDI("Paste.png");
            pasteToolStripMenuItem.Click += (s, a) => m_TableEditViewModel.PasteCells();

            var cellContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            cellContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                copyToolStripMenuItem,
                cutToolStripMenuItem,
                pasteToolStripMenuItem });
            this.reoGridControl.ContextMenuStrip = cellContextMenuStrip;
        }

        private string CopyMenuItemText => Properties.Resources.MenuEdit_Copy + "(Ctrl+C)";
        private string CutMenuItemText => Properties.Resources.MenuEdit_Cut + "(Ctrl+X)";
        private string PasteMenuItemText => Properties.Resources.MenuEdit_Paste + "(Ctrl+V)";

        private System.Drawing.Image ImageWpfToGDI(string image_name)
        {
            return ImageWpfToGDI(Util.Util.ImageFromResource(image_name));
        }

        private System.Drawing.Image ImageWpfToGDI(System.Windows.Media.Imaging.BitmapImage image)
        {
            MemoryStream ms = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }
    }
}
