using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OnlineRestaurant.ViewModels
{
    public class BaseVM : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // Cleanup managed resources
                OnDispose();
            }

            // Cleanup unmanaged resources

            _isDisposed = true;
        }

        // This is called by Dispose(bool) to allow derived classes to override disposal logic
        // without having to override the entire Dispose pattern
        protected virtual void OnDispose()
        {
            // No resources to clean up in the base class
        }

        ~BaseVM()
        {
            Dispose(false);
        }
    }
}
