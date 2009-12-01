using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ApplicationChooser
{
    public class AppItemViewModel : INotifyPropertyChanged
    {
        private readonly AppItem appItem;
        public AppItem AppItem
        {
            get { return appItem;}
        }

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return !IsOptional || isSelected || SubApps.Exists(s => s.IsSelected);
            }
            set
            {
                isSelected = value;
                SubApps.ForEach(c => c.IsSelected = value);

                //select parent when child selected
                if (Parent != null && value)
                    Parent.Select();

                OnPropertyChanged("IsSelected");
            }
        }

        public bool IsOptional
        {
            get { return !appItem.IsRequired && !SubApps.Exists(s => s.appItem.IsRequired); }
        }

        public Visibility Visibility { get; set; }

        public string Name
        {
            get { return appItem.Name; }
        }

        private AppItemViewModel parent;
        public AppItemViewModel Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                OnPropertyChanged("Parent");
            }
        }

        private readonly List<AppItemViewModel> subApps = new List<AppItemViewModel>();
        public List<AppItemViewModel> SubApps
        {
            get { return subApps; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppItemViewModel(AppItem item)
        {
            appItem = item;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Select()
        {
            isSelected = true;
            OnPropertyChanged("IsSelected");
        }
    }
}