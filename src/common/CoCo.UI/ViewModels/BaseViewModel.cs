using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoCo.UI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

        // TODO: avoid boxing for value types
        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, default) || !oldValue.Equals(newValue))
            {
                oldValue = newValue;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}