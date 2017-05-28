using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Globalization;
using ICSharpCode.AvalonEdit;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Gherkin.Util;
using Gherkin.ViewModel;
using System.Windows.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IAppSettings m_AppSettings;
        private GherkinViewModel m_ViewModel;
        private InteropMessageReceiver m_InteropMessageReceiver;
        private GherkinKeywordsViewModel m_GherkinKeywordsViewModel;
        private BetterWpfControls.MenuButton m_ShowQuickLinksButton;

        public MainWindow(IAppSettings appSettings, GherkinViewModel viewModel, GherkinKeywordsViewModel gherkinKeywordsViewModel, TableEditViewModel tableEditViewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);  // コンバータのCultureInfo型のプロパティを使ってローカライズする時は、
                                                                                                // プログラムの言語設定をしておく必要があります。
            m_AppSettings = appSettings;
            m_ViewModel = viewModel;
            m_GherkinKeywordsViewModel = gherkinKeywordsViewModel;
            m_ViewModel.EditorTabControl = this.editorTabControl;
            m_ViewModel.SetMessageTextEditor(this.messageTextEditor);
            this.tableEditorGrid.SetTableEditViewModel(tableEditViewModel);

            InitWindowSize();

            this.Deactivated += OnWindowDeactivated;
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            EventAggregator<WindowDeactivatedArg>.Instance.Publish(this, new WindowDeactivatedArg());
        }

        public string OpenFeatureFile
        {
            set { m_ViewModel.StartupFile(value); }
        }

        private void InitWindowSize()
        {
            this.Width = m_AppSettings.MainWindowSize.Width;
            this.Height = m_AppSettings.MainWindowSize.Height;

            if (m_AppSettings.IsMainWindowStateMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void recentFilesMenuItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!HaveRecentFilesChanged()) return;

            this.recentFilesMenuItem.Items.Clear();
            int max_items = ConfigReader.GetValue<int>("max_recent_files_for_submenu", 20);
            foreach (var file in m_AppSettings.RecentFilesInfo.Take(max_items))
            {
                MenuItem newSubMenuItem = new MenuItem();
                newSubMenuItem.Header = file.FilePath;
                newSubMenuItem.Command = m_ViewModel.OpenRecentFileCmd;
                newSubMenuItem.CommandParameter = file.FilePath;
                this.recentFilesMenuItem.Items.Add(newSubMenuItem);
            }
        }

        private bool HaveRecentFilesChanged()
        {
            int max_items = ConfigReader.GetValue<int>("max_recent_files_for_submenu", 20);
            if ((this.recentFilesMenuItem.Items.Count < max_items) &&
                (m_AppSettings.RecentFilesInfo.Count >= max_items)) return true;

            for (int i = 0; i < this.recentFilesMenuItem.Items.Count && i < m_AppSettings.RecentFilesInfo.Count; i++)
            {
                MenuItem subMenuItem = this.recentFilesMenuItem.Items[i] as MenuItem;
                if ((string)subMenuItem.Header != m_AppSettings.RecentFilesInfo[i].FilePath) return true;
            }

            return false;
        }

        private void OnFindAndReplace(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_ViewModel != null)
            {
                m_ViewModel.ShowFindReplace();
            }
            e.Handled = true;
        }

        private void CanExecuteEditorCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (m_ViewModel?.HasEditorLoaded == true);
            e.Handled = true;
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnNewFile();
            e.Handled = true;
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnOpenFile();
            e.Handled = true;
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnSaveFile();
            e.Handled = true;
        }

        private void OnSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnSaveAsFile();
            e.Handled = true;
        }

        private void OnPrint(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnPrint();
            e.Handled = true;
        }

        private void OnPrintPreview(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel.OnPrintPreview();
            e.Handled = true;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = m_ViewModel.SaveAllFilesWithRequesting();
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            m_ViewModel.SaveLastSelectedFile();
            m_ViewModel.SaveLastOpenedFileInfo();
            m_AppSettings.IsMainWindowStateMaximized = (WindowState == WindowState.Maximized);
            if (WindowState == WindowState.Normal)
            {
                m_AppSettings.MainWindowSize = new Size(Width, Height);
            }
            Properties.Settings.Default.Save();
            m_AppSettings.Save();

            Application.Current.Shutdown(); // It will kill all threads that opened by application
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                m_ViewModel.OpenFiles(files);
            }
        }

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) ||
                e.Data.GetDataPresent(DataFormats.UnicodeText, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            m_ShowQuickLinksButton = editorTabControl
                                        .Template
                                        .FindName("PART_QuickLinksHost", editorTabControl) as BetterWpfControls.MenuButton;
            m_ShowQuickLinksButton.ToolTip = Properties.Resources.Tooltip_OpenedDocuments;
        }

        /// <summary>
        /// Initialize InteropMessageReceiver window but do not show it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            m_InteropMessageReceiver = new InteropMessageReceiver(this, m_ViewModel);
            m_InteropMessageReceiver.InitHwnd();

            m_ViewModel.IsAllowRunningMultiApps = false;
        }

        private void OnGherkinKeywordsMenuClicked(object sender, RoutedEventArgs e)
        {
            GherkinKeywords help = new GherkinKeywords(m_GherkinKeywordsViewModel);
            help.Owner = this;
            help.Show();
        }

        private void OnShwoCalculator(object sender, RoutedEventArgs e)
        {
            var calculator = new Calculator();
            calculator.Owner = this;
            calculator.Show();
        }
    }
}
