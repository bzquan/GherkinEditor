using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public static class GherkinEditerCommand
    {
        private static RoutedUICommand _ShowSubEditorCmd = new RoutedUICommand("ShowSubEditorCmd", "ShowSubEditorCmd", typeof(GherkinEditerCommand));
        private static RoutedUICommand _HideSubEditorCmd = new RoutedUICommand("HideSubEditorCmd", "HideSubEditorCmd", typeof(GherkinEditerCommand));

        private static RoutedUICommand _ShowViewerEditorCmd = new RoutedUICommand("ShowViewerEditorCmd", "ShowViewerEditorCmd", typeof(GherkinEditerCommand));
        private static RoutedUICommand _HideViewerEditorCmd = new RoutedUICommand("HideViewerEditorCmd", "HideViewerEditorCmd", typeof(GherkinEditerCommand));

        /// <summary>
        /// Show sub editor command
        /// </summary>
        public static RoutedUICommand ShowSubEditorCmd { get { return _ShowSubEditorCmd; } }

        /// <summary>
        /// Hide sub editor command
        /// </summary>
        public static RoutedUICommand HideSubEditorCmd { get { return _HideSubEditorCmd; } }

        /// <summary>
        /// Show viewer editor command
        /// </summary>
        public static RoutedUICommand ShowViewerEditorCmd { get { return _ShowViewerEditorCmd; } }

        /// <summary>
        /// Hide viewer editor command
        /// </summary>
        public static RoutedUICommand HideViewerEditorCmd { get { return _HideViewerEditorCmd; } }

        /// <summary>
        /// Static constructor.
        /// Register all commands
        /// </summary>
        static GherkinEditerCommand()
        {
            // Register CommandBinding for all windows.
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ShowSubEditorCmd, ShowSubEditorCmd_Executed, ShowSubEditorCmd_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(HideSubEditorCmd, HideSubEditorCmd_Executed, HideSubEditorCmd_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ShowViewerEditorCmd, ShowViewerEditorCmd_Executed, ShowViewerEditorCmd_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(HideViewerEditorCmd, HideViewerEditorCmd_Executed, HideViewerEditorCmd_CanExecute));
        }

        public static GherkinViewModel GherkinViewModel { get; set; }

        private static void ShowSubEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowHSplitView = true;
            e.Handled = true;
        }

        private static void ShowSubEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (GherkinViewModel.ShowHSplitView == false);
        }

        private static void HideSubEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowHSplitView = false;
            e.Handled = true;
        }

        private static void HideSubEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GherkinViewModel.ShowHSplitView;
        }

        private static void ShowViewerEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowVSplitView = true;
            e.Handled = true;
        }

        private static void ShowViewerEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (GherkinViewModel.ShowVSplitView == false);
        }

        private static void HideViewerEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowVSplitView = false;
            e.Handled = true;
        }

        private static void HideViewerEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GherkinViewModel.ShowVSplitView;
        }
    }
}