using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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

        private void btnCollapseAllGroups_ButtonClick(object sender, RoutedEventArgs e)
        {
            CollapseOrExpandAll(null, true);
        }

        private void btnExpandAllGroups_ButtonClick(object sender, RoutedEventArgs e)
        {
            CollapseOrExpandAll(null, false);
        }

        private void CollapseOrExpandAll(CollectionViewGroup inputGroup, Boolean bCollapseGroup)
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
                    // Expand/Collapse current group
                    if (bCollapseGroup)
                        DataGrid.CollapseGroup(group);
                    else
                        DataGrid.ExpandGroup(group);

                    // Recursive Call for SubGroups
                    if (!group.IsBottomLevel)
                        CollapseOrExpandAll(group, bCollapseGroup);
                }
            }
        }
    }
}
