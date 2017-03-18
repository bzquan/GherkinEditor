using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gherkin.ViewModel
{
    class VisualChildrenFinder
    {
        public static T FindControl<U, T>(U parent, string name)
            where T : DependencyObject
            where U : DependencyObject
        {
            foreach (var v in Find<T>(parent))
            {
                string property_name = v.GetType().GetProperty("Name").GetValue(v, null) as string;
                if (property_name == name)
                {
                    return v as T;
                }
            }
            return null;
        }

        public static IEnumerable<T> Find<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in Find<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
