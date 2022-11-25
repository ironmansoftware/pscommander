using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using Swordfish.NET.Collections;

namespace pscommander
{
    public class DataSource : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ScriptBlock LoadData { get; set; }
        public int RefreshInterval { get; set; }
        public object[] ArgumentList { get; set; }

        private object _currentValue;
        public object CurrentValue { 
            get {
                return _currentValue;
            }
            set {
                _currentValue = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValue)));
            }
        }

        public int HistoryLimit { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableDictionary<DateTime, object> History { get; set; } =  new ObservableDictionary<DateTime, object>();
    }
}