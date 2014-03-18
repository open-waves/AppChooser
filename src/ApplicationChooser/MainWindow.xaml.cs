using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace ApplicationChooser
{
    public partial class MainWindow : Window
    {
// ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;
        public IList<AppItemViewModel> Items { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadApps();
            DataContext = this;

            // Trying to solve problem with frozen white screen which happens occasionally at startup.
            _timer = new Timer(
                state => Dispatcher.Invoke((Action)(UpdateLayout)), 
                null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }

        public void LoadApps()
        {
            try
            {
                var configFilePath = string.Empty;
                if (App.Args.Count > 0)
                    configFilePath = App.Args[0];

                configFilePath = string.IsNullOrEmpty(configFilePath) ? "apps.xml" : configFilePath;
                var config = XDocument.Load(configFilePath);
                Items = config.Element("apps").Elements("app").Select(GetAppNode).ToList();
            }
            catch (FileNotFoundException ex)
            {
                LogAndInformUser("could not find xml file", ex);
            }
            catch (XmlException ex)
            {
                LogAndInformUser("invalid xml file structure", ex);
            }
            catch (NullReferenceException ex)
            {
                LogAndInformUser("invalid xml file structure", ex);
            }
            catch (Exception ex)
            {
                Log.WriteLine("Unknown error", ex);
                Close();
            }
        }

        private void LogAndInformUser(string message, Exception ex)
        {
            Log.WriteLine(message, ex);
            MessageBox.Show(message, "Error");
            Close();
        }

        private AppItemViewModel GetAppNode(XElement appNode)
        {
            return GetAppNode(appNode, false);
        }

        private AppItemViewModel GetAppNode(XElement appNode, bool isParentSelected)
        {
            var item = new AppItemViewModel(new AppItem
                                     {
                                         Name = (string)appNode.Attribute("name"),
                                         Command = (string)appNode.Attribute("command"),
                                         Arguments = (string)appNode.Attribute("arguments"),
                                         IsRequired = (bool?)appNode.Attribute("required") ?? false
                                     });

            var isCurrentSelected = ((bool?)appNode.Attribute("selected") ?? false);
            item.IsSelected = isCurrentSelected || isParentSelected;

            foreach (var node in appNode.Elements("app"))
            {
                var child = GetAppNode(node, isCurrentSelected);
                child.Parent = item;
                item.SubApps.Add(child);
            }

            return item;
        }

        private void Execute()
        {
            var selectedItems = GetItemsToExecute(Items);
            var i = 0.0;
            var failedItems = new List<AppItem>();
            foreach (var itemView in selectedItems)
            {
                try
                {
                    var process = Process.Start(itemView.AppItem.Command, itemView.AppItem.Arguments);
                    if (process != null)
                    {
                        UpdateStatus(itemView.AppItem.Name, (i * 100) / selectedItems.Count);
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(string.Format("Could not execute item {0}", itemView.AppItem.Name), ex);
                    failedItems.Add(itemView.AppItem);
                }

                i++;
            }

            UpdateStatus("Finished", 100);

            if (failedItems.Count > 0)
                MessageBox.Show(string.Format("Could not execute the following items:\n{0}",
                                              string.Join("\n", failedItems.Select(it => it.Name).ToArray())), "Error");
        }

        private List<AppItemViewModel> GetItemsToExecute(IEnumerable<AppItemViewModel> itemViewModels)
        {
            var apps = new List<AppItemViewModel>();
            foreach (var model in itemViewModels)
            {
                if (model.IsSelected)
                {
                    if (string.IsNullOrWhiteSpace(model.AppItem.Command) == false)
                        apps.Add(model);
                        
                    apps.AddRange(GetItemsToExecute(model.SubApps));
                }
            }

            return apps;
        }

        private void UpdateStatus(string statusText, double value)
        {
            this.Dispatcher.Invoke((Action)(() =>
                                                 {
                                                     progressLabel.Text = statusText;
                                                     progressBar.Value = value;
                                                 }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            Thread thread = new Thread(Execute);
            thread.Start();
            IsEnabled = false;
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Items)
                item.IsSelected = true;
        }

        private void deSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }
    }
}
