using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.DataGrid;
using static FCSModdingUtility.DI;

namespace FCSModdingUtility
{
    public class EditorPageViewModel : BaseViewModel
    {
        public ModItemViewModel ModItemViewModel { get; set; }
        public ICommand BackBTNCommand { get; set; }
        public ICommand InvalidFilesFilterBTNCommand { get; set; }
        public ICommand CollapseTreeviewBTNCommand { get; set; }
        public ICommand ExpandTreeviewBTNCommand { get; set; }
        public ICommand RefreshBTNCommand { get; set; }

        private readonly string[] _allowedExtention = {".json", ".png", ".dll", ".txt",""};

        public EditorPageViewModel()
        {

        }

        private void BackBTNCommandMethod(object o)
        {
            ModItemViewModel.UpdateElement();

            ViewModelApplication.GoToPage(ApplicationPage.StartPage);
        }

        public EditorPageViewModel(ModItemViewModel vm)
        {
            ModItemViewModel = vm;
            BackBTNCommand = new RelayParameterizedCommand(BackBTNCommandMethod);
            InvalidFilesFilterBTNCommand = new RelayParameterizedCommand(InvalidFilesFilterBTNCommandMethod);
            CollapseTreeviewBTNCommand = new RelayParameterizedCommand(CollapseTreeviewCommandMethod);
            ExpandTreeviewBTNCommand = new RelayParameterizedCommand(ExpandTreeviewCommandMethod);
            RefreshBTNCommand = new RelayCommand(RefreshCommandMethod);
        }

        private void InvalidFilesFilterBTNCommandMethod(object o)
        {
            var treeview = (TreeView)o;
            foreach (DirectoryItemViewModel item in treeview.Items)
                DisableInvalid(item);
        }

        private void DisableInvalid(DirectoryItemViewModel vm)
        {
            foreach (DirectoryItemViewModel item in vm.Children)
            {
                if (item == null || item.Type != DirectoryItemType.File) continue;

                if(_allowedExtention.Contains(item.GetFileExtention())) continue;
                item.IsChecked = false;

                if (item.HasItems)
                    CollapseTreeviewItems(item);
            }
        }

        private void RefreshCommandMethod()
        {
            ModItemViewModel.UpdateTreeView();
        }

        private void CollapseTreeviewCommandMethod(object o)
        {
            var treeview = (TreeView) o;
            foreach (DirectoryItemViewModel item in treeview.Items)
                CollapseTreeviewItems(item);

            #region Oint
            //IList<Object> groupSubGroups = null;

            //If top level then inputGroup will be null
            //if (inputGroup == null)
            //{
            //    if (DataGrid.Items.Groups != null)
            //        groupSubGroups = DataGrid.Items.Groups;
            //}
            //else
            //{
            //    groupSubGroups = inputGroup.GetItems();
            //}

            //if (groupSubGroups != null)
            //{

            //    foreach (CollectionViewGroup group in groupSubGroups)
            //    {
            //        if (group.Items.Count < 0) continue;

            //        if (group.Items[0].GetType() == typeof(DirectoryItemViewModel))
            //        {
            //            ProcessGroup(group);
            //        }
            //        else
            //        {
            //            foreach (CollectionViewGroup group1 in group.Items)
            //            {
            //                ProcessGroup(group1);
            //            }
            //        }

            //        Expand / Collapse current group
            //        if (bCollapseGroup)
            //            DataGrid.CollapseGroup(group);
            //        else
            //            DataGrid.ExpandGroup(group);

            //        Recursive Call for SubGroups
            //        if (!group.IsBottomLevel)
            //                CollapseOrExpandAll(group);
            //    }
            //} 
            #endregion
        }

        private void ExpandTreeviewCommandMethod(object o)
        {
            var treeview = (TreeView)o;
                        foreach (DirectoryItemViewModel item in treeview.Items)
                            ExpandTreeviewItems(item);
        }

        private void CollapseTreeviewItems(DirectoryItemViewModel vm)
        {
            if (!vm.IsRoot)
            {
                vm.IsExpanded = false;
            }

            foreach (DirectoryItemViewModel item in vm.Children)
            {
                if(item == null) continue;

                item.IsExpanded = false;

                if (item.HasItems)
                    CollapseTreeviewItems(item);
            }
        }

        private void ExpandTreeviewItems(DirectoryItemViewModel vm = null)
        {
            if (!vm.IsRoot)
            {
                vm.IsExpanded = true;
            }


            foreach (DirectoryItemViewModel item in vm.Children)
            {
                if (item == null) continue;

                item.IsExpanded = true;

                if (item.HasItems)
                    ExpandTreeviewItems(item);
            }
        }

        public ObservableCollection<DirectoryItemViewModel> Items { get; set; }
    }
}
