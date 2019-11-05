using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace FCSModdingUtility
{
    /// <summary>
    /// Interaction logic for EditorPage.xaml
    /// </summary>
    public partial class EditorPage : BasePage<EditorPageViewModel>
    {
        public EditorPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        public EditorPage(EditorPageViewModel specificViewModel) : base(specificViewModel)
        {
            InitializeComponent();
        }

        private void OnPreparePropertyItem(object sender, PropertyItemEventArgs e)
        {
            var propertyItem = e.PropertyItem as PropertyItem;
            // Parent of top-level properties is the PropertyGrid itself.
            bool isTopLevelProperty =
                (propertyItem.ParentElement is Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid);
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = (DependencyObject)e.OriginalSource;
            var item = (TreeViewItem)GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem));
            var selectedElement = (DirectoryItemViewModel)item.Header;
            item.IsSelected = true;
            DirectoryItemType header = selectedElement.Type;

            if (header == DirectoryItemType.Folder)
            {
                MenuItem mnuItem1 = new MenuItem { Header = "Add New File" };
                MenuItem mnuItem2 = new MenuItem { Header = "Reveal in Explorer" };
                MenuItem mnuItem3 = new MenuItem { Header = "Delete Directory" };

                ContextMenu menu = new ContextMenu() { };
                menu.Items.Add(mnuItem1);
                menu.Items.Add(mnuItem2);
                menu.Items.Add(mnuItem3);

                mnuItem1.Click += MnuItem1_Click;
                mnuItem2.Click += MnuItem2_Click;
                mnuItem3.Click += MnuItem3_Click;

                ((TreeViewItem)sender).ContextMenu = menu;
            }
            else if (header == DirectoryItemType.File)
            {
                ContextMenu menu1 = new ContextMenu() { };

                MenuItem mnuItem5 = new MenuItem { Header = "Reveal in Explorer" };
                MenuItem mnuItem6 = new MenuItem { Header = "Delete File" };

                mnuItem5.Click += MnuItem5_Click;
                mnuItem6.Click += MnuItem6_Click;

                menu1.Items.Add(mnuItem5);
                menu1.Items.Add(mnuItem6);

                ((TreeViewItem)sender).ContextMenu = menu1;
            }
        }

        private void MnuItem3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult diag =
                    MessageBox.Show("This directory will be deleted and cant be undone would you like to continue?",
                        "Delete Directory", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (diag == MessageBoxResult.Yes)
                {
                    Directory.Delete(GetDVMFromTreeItem(sender).FullPath);
                    DeleteObject(((EditorPageViewModel) DataContext).Items, GetDVMFromTreeItem(sender).FullPath);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void DeleteObject(ObservableCollection<DirectoryItemViewModel> listVM, string path)
        {
            foreach (var item in listVM)
            {
                if (item != null)
                {
                    if (item.FullPath == path)
                    {
                        listVM.Remove(item);
                        break;
                    }

                    if (item.ChildrenCount != 0)
                    {
                        DeleteObject(item.Children, path);
                    }
                }
            }
        }

        private void MnuItem2_Click(object sender, RoutedEventArgs e)
        {
            string argument = "/select, \"" + GetDVMFromTreeItem(sender).FullPath + "\"";

            Process.Start("explorer.exe", argument);
        }

        private void MnuItem1_Click(object sender, RoutedEventArgs e)
        {
            var directoryItemVM = GetDVMFromTreeItem(sender);

            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var filename = Path.GetFileName(ofd.FileName);
                    var destPath = Path.Combine(directoryItemVM.FullPath, filename);
                    directoryItemVM.AddNewFile(ofd.FileName, destPath,directoryItemVM.IsRoot);
                }
            }
        }
        
        private void MnuItem6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult diag =
                    MessageBox.Show("This file will be deleted and cant be undone would you like to continue?",
                        "Delete File", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (diag == MessageBoxResult.Yes)
                {
                    var file = GetDVMFromTreeItem(sender).FullPath;
                    File.Delete(file);
                    DeleteObject(((EditorPageViewModel)DataContext).Items, file);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void MnuItem5_Click(object sender, RoutedEventArgs e)
        {
            string argument = "/select, \"" + GetDVMFromTreeItem(sender).FullPath + "\"";

            Process.Start("explorer.exe", argument);
        }

        private DirectoryItemViewModel GetDVMFromTreeItem(object sender)
        {
            var cmb = (MenuItem)sender;
            return (DirectoryItemViewModel)cmb.DataContext;
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }
    }
}
