using ErXBoutCode.MVVM;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ErXZEService.ViewModels
{
    public class BaseViewModel : ViewModel
    {
        bool isBusy = false;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            PropChanged(propertyName);
            return true;
        }
    }
}
