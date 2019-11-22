namespace FCSModdingUtility
{
    /// <summary>
    /// Interaction logic for NewProjectPage.xaml
    /// </summary>
    public partial class NewProjectPage
    {
        public NewProjectPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        public NewProjectPage(NewProjectViewModel specificViewModel) : base(specificViewModel)
        {
            InitializeComponent();
        }
    }
}
