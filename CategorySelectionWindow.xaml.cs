using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyTools.Services;

namespace MyTools
{
    /// <summary>
    /// Window for selecting categories and template view for isolated view generation.
    /// </summary>
    public partial class CategorySelectionWindow : Window
    {
        public ObservableCollection<CategoryItem> AllCategories { get; }
        public ObservableCollection<ViewItem> AllViews { get; }
        public List<CategoryItem> SelectedCategories { get; private set; }
        public ViewItem? SelectedView { get; private set; }
        public ViewItem? CurrentActiveView { get; }
        public bool CreateViewClicked { get; private set; }

        public CategorySelectionWindow(List<CategoryItem> categories, List<ViewItem> views, ViewItem currentActiveView)
        {
            InitializeComponent();

            AllCategories = new ObservableCollection<CategoryItem>(categories);
            AllViews = new ObservableCollection<ViewItem>(views);
            CurrentActiveView = currentActiveView;
            SelectedCategories = new List<CategoryItem>();

            CategoryListBox.ItemsSource = AllCategories;
            ViewComboBox.ItemsSource = AllViews;
            ViewComboBox.DisplayMemberPath = "Name";

            UpdateCreateViewButton();
            UpdateCurrentViewDisplay();
            UpdateSelectionSetLabels();
        }

        #region Category Selection

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.ToLower();

            CategoryListBox.ItemsSource = string.IsNullOrWhiteSpace(filter)
                ? AllCategories
                : AllCategories.Where(c => c.Name.ToLower().Contains(filter)).ToList();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllCategoriesSelection(true);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllCategoriesSelection(false);
        }

        private void SetAllCategoriesSelection(bool isSelected)
        {
            foreach (var item in AllCategories)
                item.IsSelected = isSelected;

            CategoryListBox.Items.Refresh();
            UpdateCreateViewButton();
        }

        private void Category_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCreateViewButton();
        }

        #endregion

        #region View Selection

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCreateViewButton();
        }

        private void SelectView_Click(object sender, RoutedEventArgs e)
        {
            ViewComboBox.IsDropDownOpen = true;
        }

        private void UpdateCurrentViewDisplay()
        {
            if (CurrentActiveView != null)
            {
                CurrentViewTextBlock.Text = CurrentActiveView.Name;
                CurrentViewTextBlock.FontStyle = FontStyles.Normal;
                CurrentViewTextBlock.Foreground = Brushes.Black;
                CurrentViewTextBlock.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                CurrentViewTextBlock.Text = "No active view";
                CurrentViewTextBlock.FontStyle = FontStyles.Italic;
                CurrentViewTextBlock.Foreground = Brushes.Gray;
            }
        }

        #endregion

        #region Selection Sets

        private void UpdateSelectionSetLabels()
        {
            UpdateSetLabel(1, Set1Label);
            UpdateSetLabel(2, Set2Label);
        }

        private void UpdateSetLabel(int setNumber, System.Windows.Controls.TextBlock label)
        {
            var set = SelectionSetService.GetSelectionSet(setNumber);

            if (set != null && set.CategoryNames.Count > 0)
            {
                label.Text = $"{set.CategoryNames.Count} categories";
                label.Foreground = Brushes.Black;
            }
            else
            {
                label.Text = "(Empty)";
                label.Foreground = Brushes.Gray;
            }
        }

        private void SaveSet1_Click(object sender, RoutedEventArgs e) => SaveSelectionSet(1);
        private void SaveSet2_Click(object sender, RoutedEventArgs e) => SaveSelectionSet(2);
        private void LoadSet1_Click(object sender, RoutedEventArgs e) => LoadSelectionSet(1);
        private void LoadSet2_Click(object sender, RoutedEventArgs e) => LoadSelectionSet(2);

        private void SaveSelectionSet(int setNumber)
        {
            var selected = AllCategories.Where(c => c.IsSelected).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one category to save.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<string> categoryNames = selected.Select(c => c.Name).ToList();
            SelectionSetService.SaveSelectionSet(setNumber, $"Set {setNumber}", categoryNames);

            UpdateSelectionSetLabels();
            MessageBox.Show($"Saved {categoryNames.Count} categories to Set {setNumber}.", "Set Saved",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSelectionSet(int setNumber)
        {
            var set = SelectionSetService.GetSelectionSet(setNumber);

            if (set == null || set.CategoryNames.Count == 0)
            {
                MessageBox.Show($"Set {setNumber} is empty. Please save a selection first.", "Empty Set",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectionSetService.ApplySelectionSet(set, AllCategories);
            CategoryListBox.Items.Refresh();
            UpdateCreateViewButton();

            MessageBox.Show($"Loaded {set.CategoryNames.Count} categories from Set {setNumber}.", "Set Loaded",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Dialog Actions

        private void UpdateCreateViewButton()
        {
            CreateViewButton.IsEnabled = AllCategories.Any(c => c.IsSelected) && ViewComboBox.SelectedItem != null;
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

        #endregion
    }

    /// <summary>
    /// Represents a category item with selection state.
    /// </summary>
    public class CategoryItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; } = string.Empty;
        public ElementId CategoryId { get; set; } = ElementId.InvalidElementId;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Represents a view item with its name and ID.
    /// </summary>
    public class ViewItem
    {
        public string Name { get; set; } = string.Empty;
        public ElementId ViewId { get; set; } = ElementId.InvalidElementId;
    }
}
