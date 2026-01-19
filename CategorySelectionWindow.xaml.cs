using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyTools
{
    public partial class CategorySelectionWindow : Window
    {
        public ObservableCollection<CategoryItem> AllCategories { get; set; }
        public ObservableCollection<ViewItem> AllViews { get; set; }
        public List<CategoryItem> SelectedCategories { get; private set; }
        public ViewItem SelectedView { get; private set; }
        public ViewItem CurrentActiveView { get; private set; } // Store current active view
        public bool CreateViewClicked { get; private set; }

        // MODIFIED CONSTRUCTOR: Accept current active view
        public CategorySelectionWindow(
            List<CategoryItem> categories,
            List<ViewItem> views,
            ViewItem currentActiveView)
        {
            InitializeComponent();

            AllCategories = new ObservableCollection<CategoryItem>(categories);
            AllViews = new ObservableCollection<ViewItem>(views);
            CurrentActiveView = currentActiveView; // Store the current active view

            CategoryListBox.ItemsSource = AllCategories;
            ViewComboBox.ItemsSource = AllViews;
            ViewComboBox.DisplayMemberPath = "Name";

            SelectedCategories = new List<CategoryItem>();
            CreateViewClicked = false;

            UpdateCreateViewButton();
            UpdateCurrentViewDisplay(); // Display the current active view
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
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
            UpdateCreateViewButton();
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in AllCategories)
            {
                item.IsSelected = false;
            }
            CategoryListBox.Items.Refresh();
            UpdateCreateViewButton();
        }

        private void Category_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCreateViewButton();
        }

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCreateViewButton();
        }

        private void SelectView_Click(object sender, RoutedEventArgs e)
        {
            // Open a separate view selection dialog if needed
            // For now, just focus the ComboBox
            ViewComboBox.IsDropDownOpen = true;
        }

        private void UpdateCreateViewButton()
        {
            bool hasSelectedCategories = AllCategories.Any(c => c.IsSelected);
            bool hasSelectedView = ViewComboBox.SelectedItem != null;

            CreateViewButton.IsEnabled = hasSelectedCategories && hasSelectedView;
        }

        // MODIFIED METHOD: Display the frozen current active view
        private void UpdateCurrentViewDisplay()
        {
            if (CurrentActiveView != null)
            {
                CurrentViewTextBlock.Text = CurrentActiveView.Name;
                CurrentViewTextBlock.FontStyle = FontStyles.Normal;
                CurrentViewTextBlock.Foreground = Brushes.Black;
                CurrentViewTextBlock.FontWeight = FontWeights.SemiBold; // Make it bold to show it's fixed
            }
            else
            {
                CurrentViewTextBlock.Text = "No active view";
                CurrentViewTextBlock.FontStyle = FontStyles.Italic;
                CurrentViewTextBlock.Foreground = Brushes.Gray;
            }
        }

        private void CreateView_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategories = AllCategories.Where(c => c.IsSelected).ToList();
            SelectedView = ViewComboBox.SelectedItem as ViewItem;
            CreateViewClicked = true;
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

    public class ViewItem
    {
        public string Name { get; set; }
        public Autodesk.Revit.DB.ElementId ViewId { get; set; }
    }
}