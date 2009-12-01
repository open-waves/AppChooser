using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace ApplicationChooser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IList<AppItemViewModel> Items { get; set;}

        public MainWindow()
        {
            InitializeComponent();
            LoadApps();
            DataContext = this;

        }

        public void LoadApps()
        {
            try
            {
                var config = XDocument.Load("apps.xml");
                Items = config.Element("apps").Elements("app").Select(n => GetAppNode(n)).ToList();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Unable to find apps.xml", "Error");
                Close();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Invalid apps.xml structure", "Error");
                Close();
            }
        }

        private AppItemViewModel GetAppNode(XElement appNode)
        {
            var item = new AppItemViewModel(new AppItem
                                     {
                                         Name = (string) appNode.Attribute("name"),
                                         Command = (string) appNode.Attribute("command"),
                                         Arguments = (string) appNode.Attribute("arguments"),
                                         IsRequired = (bool?) appNode.Attribute("required") ?? false
                                     });

            var isVisible = (bool?) appNode.Attribute("visible") ?? true;
            item.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

            if (!isVisible)
                item.IsSelected = true;

            foreach (var node in appNode.Elements("app"))
            {
                var child = GetAppNode(node);
                child.Parent = item;
                item.SubApps.Add(child);
            }

            return item;
        }

        private void Execute()
        {
            var selectedItems = GetItemsToExecute(Items.Where(it => it.IsSelected));
            var i = 0;
            var failedItems = new List<AppItem>();
            foreach (var itemView in selectedItems)
            {
                UpdateStatus(itemView.AppItem, (Convert.ToDouble(++i) * 100) / selectedItems.Count());

                try
                {
                    var process = Process.Start(itemView.AppItem.Command, itemView.AppItem.Arguments);
                    if (process != null) 
                        process.WaitForExit();
                }
                catch (Exception ex)
                {
                    failedItems.Add(itemView.AppItem);
                    //TODO: Add logging
                }
            }

            if (failedItems.Count > 0)
                MessageBox.Show(string.Format("Could not execute the following items:\n{0}",
                                              string.Join("\n", failedItems.Select(it => it.Name).ToArray())), "Error");

            Close();
        }

        private List<AppItemViewModel> GetItemsToExecute(IEnumerable<AppItemViewModel> itemViewModels)
        {
            var apps = new List<AppItemViewModel>();
            foreach (var model in itemViewModels)
            {
                apps.Add(model);
                apps.AddRange(GetItemsToExecute(model.SubApps));
            }

            return apps;
        }

        private void UpdateStatus(AppItem currentItem, double value)
        {
            progressLabel.Text = currentItem.Name;
            progressBar.Value = value;

            //pretty...useless
            //var duration = new Duration(TimeSpan.FromSeconds(5));
            //var doubleanimation = new DoubleAnimation(value, duration);
            //progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            Execute();
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
