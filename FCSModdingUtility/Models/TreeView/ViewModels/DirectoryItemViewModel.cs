using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;


namespace FCSModdingUtility
{
    /// <summary>
    /// A view model for each directory item
    /// </summary>
    public class DirectoryItemViewModel : BaseViewModel
    {
        #region Public Properties
        /// <summary>
        /// The amount of children
        /// </summary>

        public int ChildrenCount { get; set; }
        /// <summary>
        /// The type of this item
        /// </summary>

        private DirectoryItemType _type;

        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                if (value == false)
                {
                    foreach (DirectoryItemViewModel child in Children)
                    {
                        if(child == null )continue;
                        child.IsChecked = false;
                    }
                }
            }
        }
        
        public Visibility Visibility { get; set; }

        public DirectoryItemType Type
        {
            get => _type;
            set
            {
                _type = value;
                Name = value == DirectoryItemType.Drive ? FullPath : DirectoryStructure.GetFileFolderName(FullPath);
                OldName = Name;
            }
        }
        
        public string ImageName => Type == DirectoryItemType.Drive ? "drive" : (Type == DirectoryItemType.File ? "file" : (IsExpanded ? "folder-open" : "folder-closed"));

        /// <summary>
        /// The full path to the item
        /// </summary>

        public string FullPath { get; set; }

        public string OldName { get; set; }

        public bool IsRoot { get; set; }

        public bool IsInRoot { get; set; }
        
        public string Name { get; set; }

        /// <summary>
        /// A list of all children contained inside this item
        /// </summary>

        public ObservableCollection<DirectoryItemViewModel> Children { get; set; }

        /// <summary>
        /// Indicates if this item can be expanded
        /// </summary>
        public bool CanExpand => Type != DirectoryItemType.File;

        /// <summary>
        /// Indicates if the current item is expanded or not
        /// </summary>
        public bool IsExpanded
        {
            get => Children?.Count(f => f != null) > 0;
            set
            {
                // If the UI tells us to expand...
                if (value)
                    // Find all children
                    Expand();
                // If the UI tells us to close
                else
                    ClearChildren();
            }
        }

        public void Refresh()
        {
            // Find all children
            Expand();
        }

        #endregion

        #region Public Commands

        /// <summary>
        /// The command to expand this item
        /// </summary>
        [JsonIgnore] public ICommand ExpandCommand { get; set; }


        [JsonIgnore] public ICommand OpenFileCommand { get; set; }
        public bool HasItems => ChildrenCount > 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fullPath">The full path of this item</param>
        /// <param name="type">The type of item</param>
        /// <param name="childrenCount"></param>
        /// <param name="modItemViewModel"></param>
        public DirectoryItemViewModel(string fullPath, DirectoryItemType type, int childrenCount, bool isInRoot = false)
        {
            // Create commands
            ExpandCommand = new RelayCommand(Expand);
            OpenFileCommand = new RelayParameterizedCommand(OpenFileMethod);
            
            // Set path and type
            FullPath = fullPath;
            Type = type;
            ChildrenCount = childrenCount;

            // Setup the children as needed
            ClearChildren();

            IsInRoot = isInRoot;
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// Removes all children from the list, adding a dummy item to show the expand icon if required
        /// </summary>
        private void ClearChildren()
        {
            // Clear items
            Children = new ObservableCollection<DirectoryItemViewModel>();

            // Show the expand arrow if we are not a file
            if (Type == DirectoryItemType.Folder && ChildrenCount > 0)
                Children.Add(null);

            if (Type == DirectoryItemType.Drive)
                Children.Add(null);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Command that handles double clicks on the tree view items
        /// </summary>
        /// <param name="obj"></param>
        private void OpenFileMethod(object obj)
        {
            try
            {
                var curObj = (DirectoryItemViewModel)obj;
                if (curObj.Type != DirectoryItemType.File)
                {
                    if (!Path.HasExtension(curObj.FullPath)) return;
                    if (curObj.FullPath != null && Path.GetExtension(curObj.FullPath).Equals(".H", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("All .H files are loaded into the ID collection list", "Invalid Request",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Cannot open file type {Path.GetExtension(curObj.FullPath)}", "Invalid Request",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DI.ViewModelApplication.SetStatus(ApplicationStatusTypes.Error, e.Message);


                MessageBox.Show($"Error has occurred:" + Environment.NewLine + e.Message,
                    DI.ViewModelSettings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        /// <summary>
        ///  Expands this directory and finds all children
        /// </summary>
        private void Expand()
        {
            try
            {
               
                // We cannot expand a file
                if (Type == DirectoryItemType.File)
                    return;

                // Find all children
                var children = DirectoryStructure.GetDirectoryContents(FullPath);

                Children = new ObservableCollection<DirectoryItemViewModel>();

                foreach (var child in children)
                {
                    var childrenCount = DirectoryStructure.GetDirectoryContents(child.FullPath);
                    Children.Add(new DirectoryItemViewModel(child.FullPath, child.Type, childrenCount.Count));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        public string GetFileExtention()
        {
            return Type == DirectoryItemType.File ? Path.GetExtension(FullPath) : String.Empty;
        }

        public void AddNewFile(string sourceFile, string destFile, bool isInRoot)
        {
            Children.Add(new DirectoryItemViewModel(destFile, DirectoryItemType.File,0, isInRoot));
            File.Copy(sourceFile, destFile);
        }

        public string GetDirectoryName()
        {
            return Path.GetFileName(Path.GetDirectoryName(FullPath));
        }
    }
}
