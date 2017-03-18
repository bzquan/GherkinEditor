using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gherkin.ViewModel
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T item, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(item, value)) return false;
            item = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void SetProperty<TTarget, TValue>(TTarget target, TValue newValue, [CallerMemberName] string targetPropertyName = null, [CallerMemberName] string propertyName = null)
        {
            System.Reflection.PropertyInfo prop = target.GetType().GetProperty(targetPropertyName);
            if (!EqualityComparer<TValue>.Default.Equals((TValue)prop.GetValue(target), newValue))
            {
                prop.SetValue(target, newValue);
                OnPropertyChanged(propertyName);
            }
        }
    }
}
