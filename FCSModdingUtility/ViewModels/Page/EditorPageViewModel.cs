using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private readonly string[] _allowedExtention = {".json", ".png", ".dll", ".txt",""};

        public EditorPageViewModel()
        {

        }

        private void BackBTNCommandMethod(object o)
        {
            var grid = (DataGridControl) o;
            ((DataGridCollectionView)grid.ItemsSource).Refresh();

            //grid.CurrentItem = grid.Items[0];
            ModItemViewModel.UpdateElement();

            ViewModelApplication.GoToPage(ApplicationPage.StartPage);
        }

        public EditorPageViewModel(ModItemViewModel vm)
        {
            ModItemViewModel = vm;
            BackBTNCommand = new RelayParameterizedCommand(BackBTNCommandMethod);
            InvalidFilesFilterBTNCommand = new RelayParameterizedCommand(InvalidFilesFilterBTNCommandMethod);
        }

        private void InvalidFilesFilterBTNCommandMethod(object obj)
        {
            DataGrid = (DataGridControl) obj;
            CollapseOrExpandAll();
        }

        public DataGridControl DataGrid { get; set; }

        private void CollapseOrExpandAll(CollectionViewGroup inputGroup = null)
        {
            IList<Object> groupSubGroups = null;

            // If top level then inputGroup will be null
            if (inputGroup == null)
            {
                if (DataGrid.Items.Groups != null)
                    groupSubGroups = DataGrid.Items.Groups;
            }
            else
            {
                groupSubGroups = inputGroup.GetItems();
            }

            if (groupSubGroups != null)
            {

                foreach (CollectionViewGroup group in groupSubGroups)
                {
                    if(group.Items.Count < 0) continue;

                    if (group.Items[0].GetType() == typeof(FileStructure))
                    {
                        ProcessGroup(group);
                    }
                    else
                    {
                        foreach (CollectionViewGroup group1 in group.Items)
                        {
                            ProcessGroup(group1);
                        }
                    }
                    
                    //// Expand/Collapse current group
                    //if (bCollapseGroup)
                    //    DataGrid.CollapseGroup(group);
                    //else
                    //    DataGrid.ExpandGroup(group);

                    // Recursive Call for SubGroups
                    if (!group.IsBottomLevel)
                        CollapseOrExpandAll(group);
                }
            }
        }

        private void ProcessGroup(CollectionViewGroup group)
        {
            foreach (FileStructure fileStructure in group.Items)
            {
                var ext = fileStructure.GetFileExtention();
                if (!_allowedExtention.Contains(ext))
                {
                    fileStructure.Include = false;
                }
            }
        }
    }
}
