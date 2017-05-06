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

        /// <summary>
        /// Show sub editor command
        /// </summary>
        public static RoutedUICommand ShowSubEditorCmd { get { return _ShowSubEditorCmd; } }

        /// <summary>
        /// Hide sub editor command
        /// </summary>
        public static RoutedUICommand HideSubEditorCmd { get { return _HideSubEditorCmd; } }

        /// <summary>
        /// Static constructor.
        /// Register all commands
        /// </summary>
        static GherkinEditerCommand()
        {
            // Register CommandBinding for all windows.
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ShowSubEditorCmd, ShowSubEditorCmd_Executed, ShowSubEditorCmd_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(HideSubEditorCmd, HideSubEditorCmd_Executed, HideSubEditorCmd_CanExecute));
        }

        public static GherkinViewModel GherkinViewModel { get; set; }

        private static void ShowSubEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowSplitView = true;
            e.Handled = true;
        }

        private static void ShowSubEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (GherkinViewModel.ShowSplitView == false);
        }

        private static void HideSubEditorCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GherkinViewModel.ShowSplitView = false;
            e.Handled = true;
        }

        private static void HideSubEditorCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GherkinViewModel.ShowSplitView;
        }
    }
}