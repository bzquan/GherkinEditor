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

        public MainWindow(IAppSettings appSettings, GherkinViewModel viewModel, GherkinKeywordsViewModel gherkinKeywordsViewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);  // コンバータのCultureInfo型のプロパティを使ってローカライズする時は、
                                                                                                // プログラムの言語設定をしておく必要があります。
            m_AppSettings = appSettings;
            m_ViewModel = viewModel;
            m_GherkinKeywordsViewModel = gherkinKeywordsViewModel;
            m_ViewModel.EditorTabControl = editorTabControl;
            gherkinKeywordsButton.Visibility = Visibility.Collapsed;
            editorTabControl.SizeChanged += OnEditorTabControlSizeChanged;

            EventAggregator<ShowCodeGenMessageWindowArg>.Instance.Event += OnShowCodeGenMessageWindow;
            EventAggregator<CurrentGherkinLanguageArg>.Instance.Event += OnGherkinLanguage;

            genCodeMessageTextBox.Height = m_AppSettings.ShowMessageWindow ? 80 : 0;
            InitWindowSize();
        }

        private void OnGherkinLanguage(object sender, CurrentGherkinLanguageArg arg)
        {
            gherkinKeywordsButton.Visibility = Visibility.Visible;
        }

        private void OnEditorTabControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            EventAggregator<AdjustMaxWidthOfEditorTabArg>.Instance.Publish(this, new AdjustMaxWidthOfEditorTabArg());
        }

        public string OpenFeatureFile
        {
            set { m_ViewModel.StartupFile(value); }
        }

        private void OnShowCodeGenMessageWindow(object sender, ShowCodeGenMessageWindowArg arg)
        {
            genCodeMessageTextBox.Height = arg.ShowMessageWindow ? 80 : 0;
            if (arg.Message.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(arg.Message);
                genCodeMessageTextBox.Text = sb.ToString();
            }
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
            foreach (string path in m_AppSettings.RecentFiles)
            {
                MenuItem newSubMenuItem = new MenuItem();
                newSubMenuItem.Header = path;
                newSubMenuItem.Command = m_ViewModel.OpenRecentFileCmd;
                newSubMenuItem.CommandParameter = path;
                this.recentFilesMenuItem.Items.Add(newSubMenuItem);
            }
        }

        private bool HaveRecentFilesChanged()
        {
            if (this.recentFilesMenuItem.Items.Count != m_AppSettings.RecentFiles.Count) return true;

            for (int i = 0; i < this.recentFilesMenuItem.Items.Count; i++)
            {
                MenuItem newSubMenuItem = this.recentFilesMenuItem.Items[i] as MenuItem;
                if ((string)newSubMenuItem.Header != m_AppSettings.RecentFiles[i]) return true;
            }

            return false;
        }

        private void OnFindAndReplace(object sender, ExecutedRoutedEventArgs e)
        {
            m_ViewModel?.FindAndReplace();
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

            m_AppSettings.IsMainWindowStateMaximized = (WindowState == WindowState.Maximized);
            if (WindowState == WindowState.Normal)
            {
                m_AppSettings.MainWindowSize = new Size(Width, Height);
            }
            Properties.Settings.Default.Save();
            m_AppSettings.Save();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            m_ViewModel.OpenFiles(files);
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
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
    }
}
