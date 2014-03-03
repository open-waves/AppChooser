using System.Collections.Generic;

namespace ApplicationChooser
{
    public class AppItemViewModel : ViewModelBase
    {
        private readonly AppItem _appItem;
        public AppItem AppItem
        {
            get { return _appItem;}
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return !IsOptional || _isSelected || SubApps.Exists(s => s.IsSelected);
            }
            set
            {
                _isSelected = value;
                SubApps.ForEach(c => c.IsSelected = value);

                //select parent when child selected
                if (Parent != null && value)
                    Parent.Select();

                OnPropertyChanged(() => IsSelected);
            }
        }

        public bool IsOptional
        {
            get { return !_appItem.IsRequired && !SubApps.Exists(s => s._appItem.IsRequired); }
        }

        public string Name
        {
            get { return _appItem.Name; }
        }

        private AppItemViewModel _parent;
        public AppItemViewModel Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
            }
        }

        private readonly List<AppItemViewModel> _subApps = new List<AppItemViewModel>();

        public List<AppItemViewModel> SubApps
        {
            get { return _subApps; }
        }

        public AppItemViewModel(AppItem item)
        {
            _appItem = item;
        }

        private void Select()
        {
            _isSelected = true;
            OnPropertyChanged(() => IsSelected);
        }
    }
}