using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Planify.ViewModels
{
    /// <summary>
    /// Minimal INotifyPropertyChanged base til dine ViewModels.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for et givent property.
        /// </summary>
        /// <param name="propertyName">Udfyldes automatisk ved kald uden argument.</param>
        protected void Raise([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Sætter en backing field og rejser PropertyChanged kun ved reel ændring.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            Raise(propertyName);
            return true;
        }
    }
}
