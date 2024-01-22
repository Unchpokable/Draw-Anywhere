using System.Windows.Controls;
using DrawAnywhere.ViewModels;

namespace DrawAnywhere.Views
{
    /// <summary>
    /// Логика взаимодействия для DirectoryBrowser.xaml
    /// </summary>
    public partial class DirectoryBrowser : UserControl
    {
        public DirectoryBrowser()
        {
            InitializeComponent();

            BrowserView.SelectionChanged += OnItemSelected;
        }

        private void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            ((DirectoryBrowserViewModel)DataContext).SetItemSelected(BrowserView.SelectedIndex);
        }
    }
}
