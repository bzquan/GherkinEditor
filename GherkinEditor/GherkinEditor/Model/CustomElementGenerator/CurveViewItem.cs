using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OxyPlot;

namespace Gherkin.Model
{
    class CurveViewItem
    {
        private CurvePlotModel m_CurvePlotModel;
        private OxyPlot.Wpf.PlotView m_PlotView;
        private BitmapImage m_BitmapImage;

        public CurveViewItem(CurvePlotModel model)
        {
            m_CurvePlotModel = model;
        }

        public string Key => m_CurvePlotModel.Key;

        public OxyPlot.Wpf.PlotView PlotView
        {
            get
            {
                if (m_PlotView == null)
                {
                    MakePlotView();
                }
                return m_PlotView;
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                if (m_BitmapImage == null)
                {
                    MakeBitmapImage();
                }

                return m_BitmapImage;
            }
        }

        private void MakeBitmapImage()
        {
            var bitmapSource = OxyPlot.Wpf.PngExporter.ExportToBitmap(m_CurvePlotModel, 600, 400, OxyPlot.OxyColors.White);
            m_BitmapImage = Util.DrawingVisualUtil.ToPNGImage(bitmapSource);
        }

        private void SaveAsPng(string filePath)
        {
            var thread = new System.Threading.Thread(() =>
            {
                OxyPlot.Wpf.PngExporter.Export(m_CurvePlotModel, filePath, 600, 400, OxyPlot.OxyColors.White);
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void MakePlotView()
        {
            m_PlotView = new OxyPlot.Wpf.PlotView();
            m_PlotView.Model = m_CurvePlotModel;
            m_PlotView.Width = 600;
            m_PlotView.Height = 400;
            m_PlotView.ActualController.UnbindMouseWheel();

            m_PlotView.Cursor = Cursors.Arrow;
        }
    }
}
