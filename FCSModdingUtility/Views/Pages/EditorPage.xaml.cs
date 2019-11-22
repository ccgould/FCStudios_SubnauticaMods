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
        private DirectoryItemViewModel _selectedElement;
        private TreeViewItem _item;

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
            _item = (TreeViewItem)GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem));
            _selectedElement = (DirectoryItemViewModel)_item.Header;
            _item.IsSelected = true;
            DirectoryItemType header = _selectedElement.Type;


            if (header == DirectoryItemType.Folder)
            {
                MenuItem mnuFileItem1 = new MenuItem { Header = "Add New File" };
                MenuItem mnuFileItem2 = new MenuItem { Header = "Reveal in Explorer" };
                MenuItem mnuFileItem4 = new MenuItem { Header = "Check All Children" };
                MenuItem mnuFileItem5 = new MenuItem { Header = "UnCheck All Children" };
             
                ContextMenu menu = new ContextMenu() { };
                menu.Items.Add(mnuFileItem1);
                menu.Items.Add(mnuFileItem2);
                
                if (!_selectedElement.IsRoot)
                {
                    MenuItem mnuFileItem3 = new MenuItem { Header = "Delete Directory" };
                    menu.Items.Add(mnuFileItem3);
                    mnuFileItem3.Click += MnuFileItem3_Click;
                }
                
                menu.Items.Add(new Separator());
                menu.Items.Add(mnuFileItem4);
                menu.Items.Add(mnuFileItem5);

                mnuFileItem1.Click += MnuFileItem1_Click;
                mnuFileItem2.Click += MnuFileItem2_Click;
                mnuFileItem4.Click += MnuFileItem4_Click;
                mnuFileItem5.Click += MnuFileItem5_Click;
                
                if (_selectedElement.IsRoot)
                {
                    MenuItem mnuFileItem6 = new MenuItem { Header = "UnCheck Invalid Types" };
                    menu.Items.Add(new Separator());
                    menu.Items.Add(mnuFileItem6);
                    mnuFileItem6.Click += MnuFileItem6_Click;
                }

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

        private void MnuFileItem6_Click(object sender, RoutedEventArgs e)
        {
            _selectedElement.UncheckInvalid();
        }

        private void MnuFileItem5_Click(object sender, RoutedEventArgs e)
        {
            foreach (DirectoryItemViewModel child in _selectedElement.Children)
            {
                CheckChildRecursion(child,false);
            }
        }

        private void CheckChildRecursion(DirectoryItemViewModel child, bool isChecked = true)
        {
            if(child == null) return;

            child.IsChecked = isChecked;
            
            if (child.Children.Count > 0)
            {
                foreach (DirectoryItemViewModel childChild in child.Children)
                {
                    CheckChildRecursion(childChild,isChecked);
                }
            }
        }

        private void MnuFileItem4_Click(object sender, RoutedEventArgs e)
        {
            foreach (DirectoryItemViewModel child in _selectedElement.Children)
            {
                CheckChildRecursion(child);
            }
        }

        private void MnuFileItem3_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult diag = 
                MessageBox.Show("This directory will be deleted and cant be undone would you like to continue?",
                    "Delete Directory", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (diag == MessageBoxResult.Yes)
            {
                ApplicationHelpers.SafeDeleteDirectory(GetDVMFromTreeItem(sender).FullPath);
                DeleteObject();
                //DeleteObject(GetDVMFromTreeItem(sender).FullPath);
            }
        }

        private void DeleteObject()
        {
            var obj = (EditorPageViewModel)DataContext;
            obj.ModItemViewModel.DeleteTreeItem(_selectedElement);

            //foreach (var item in listVM)
            //{
            //    if (item != null)
            //    {
            //        if (item.FullPath == path)
            //        {
            //            listVM.Remove(item);
            //            break;
            //        }

            //        if (item.ChildrenCount != 0)
            //        {
            //            DeleteObject(item.Children, path);
            //        }
            //    }
            //}
        }

        private void MnuFileItem2_Click(object sender, RoutedEventArgs e)
        {
            string argument = "/select, \"" + GetDVMFromTreeItem(sender).FullPath + "\"";

            Process.Start("explorer.exe", argument);
        }

        private void MnuFileItem1_Click(object sender, RoutedEventArgs e)
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
                    DeleteObject();
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

        private ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
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
