﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Threading;

namespace ApplicationChooser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IList<AppItemViewModel> Items { get; set; }

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
                var configFilePath = string.Empty;
                if (App.Args.Count > 0)
                    configFilePath = App.Args[0];

                configFilePath = string.IsNullOrEmpty(configFilePath) ? "apps.xml" : configFilePath;
                var config = XDocument.Load(configFilePath);
                Items = config.Element("apps").Elements("app").Select(n => GetAppNode(n)).ToList();
            }
            catch (FileNotFoundException ex)
            {
                Log.WriteLine("could not find apps.xml", ex);
                MessageBox.Show("Unable to find apps.xml", "Error");
                Close();
            }
            catch (NullReferenceException ex)
            {
                Log.WriteLine("invalid apps.xml structure", ex);
                MessageBox.Show("Invalid apps.xml structure", "Error");
                Close();
            }
            catch (Exception ex)
            {
                Log.WriteLine("Unknown error", ex);
                Close();
            }
        }

        private AppItemViewModel GetAppNode(XElement appNode)
        {
            var item = new AppItemViewModel(new AppItem
                                     {
                                         Name = (string)appNode.Attribute("name"),
                                         Command = (string)appNode.Attribute("command"),
                                         Arguments = (string)appNode.Attribute("arguments"),
                                         IsRequired = (bool?)appNode.Attribute("required") ?? false
                                     });

            var isVisible = (bool?)appNode.Attribute("visible") ?? true;
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
                try
                {
                    // skip empty commands

                    


                    if (string.IsNullOrEmpty(itemView.AppItem.Command))
                        continue;

                    var process = Process.Start(itemView.AppItem.Command, itemView.AppItem.Arguments);
                    
                    if (process != null)
                        process.WaitForExit();
                    UpdateStatus(itemView.AppItem, (Convert.ToDouble(++i) * 100) / selectedItems.Count());
                    
                }
                catch (Exception ex)
                {
                    Log.WriteLine(string.Format("Could not execute item {0}", itemView.AppItem.Name), ex);
                    failedItems.Add(itemView.AppItem);
                }
            }

            if (failedItems.Count > 0)
                MessageBox.Show(string.Format("Could not execute the following items:\n{0}",
                                              string.Join("\n", failedItems.Select(it => it.Name).ToArray())), "Error");

            
            
            
        }

        private List<AppItemViewModel> GetItemsToExecute(IEnumerable<AppItemViewModel> itemViewModels)
        {
            var apps = new List<AppItemViewModel>();
            foreach (var model in itemViewModels)
            {
                if (model.SubApps.All(x => x.IsSelected))
                {
                    apps.Add(model);
                }
                apps.AddRange(GetItemsToExecute(model.SubApps));
            }

            return apps;
        }

        private void UpdateStatus(AppItem currentItem, double value)
        {
            this.Dispatcher.Invoke((Action) (() =>
                                                 {
                                                     progressLabel.Text = currentItem.Name;
                                                     progressBar.Value = value;
                                                 }));
            
            
            //pretty...useless
            //var duration = new Duration(TimeSpan.FromSeconds(5));
            //var doubleanimation = new DoubleAnimation(value, duration);
            //progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            Thread thread = new Thread(Execute);
            thread.Start();
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
