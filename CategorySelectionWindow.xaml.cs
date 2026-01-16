using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MyTools
{
    public partial class CategorySelectionWindow : Window
    {
        public ObservableCollection<CategoryItem> AllCategories { get; set; }
        public List<CategoryItem> SelectedCategories { get; private set; }

        private List<CategoryItem> _originalList;

        public CategorySelectionWindow(List<CategoryItem> categories)
        {
            InitializeComponent();

            _originalList = categories;
            AllCategories = new ObservableCollection<CategoryItem>(categories);
            CategoryListBox.ItemsSource = AllCategories;
            SelectedCategories = new List<CategoryItem>();
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(filter))
            {
                CategoryListBox.ItemsSource = AllCategories;
            }
            else
            {
                var filtered = AllCategories.Where(c => c.Name.ToLower().Contains(filter)).ToList();
                CategoryListBox.ItemsSource = filtered;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in AllCategories)
            {
                item.IsSelected = true;
            }
            CategoryListBox.Items.Refresh();
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in AllCategories)
            {
                item.IsSelected = false;
            }
            CategoryListBox.Items.Refresh();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategories = AllCategories.Where(c => c.IsSelected).ToList();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class CategoryItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public Autodesk.Revit.DB.ElementId CategoryId { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
