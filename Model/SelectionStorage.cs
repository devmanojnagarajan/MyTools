using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools
{
    public static class SelectionStorage
    {
        private static List<CategoryItem> _selectedCategories = new List<CategoryItem>();
        private static ViewItem _selectedView = null;

        // Store selected categories
        public static void SetSelectedCategories(List<CategoryItem> categories)
        {
            _selectedCategories = categories ?? new List<CategoryItem>();
        }

        // Retrieve selected categories
        public static List<CategoryItem> GetSelectedCategories()
        {
            return _selectedCategories;
        }

        // Store selected view
        public static void SetSelectedView(ViewItem view)
        {
            _selectedView = view;
        }

        // Retrieve selected view
        public static ViewItem GetSelectedView()
        {
            return _selectedView;
        }

        // Clear all selections
        public static void ClearSelections()
        {
            _selectedCategories.Clear();
            _selectedView = null;
        }

        // Check if selections exist
        public static bool HasSelections()
        {
            return _selectedCategories.Count > 0 && _selectedView != null;
        }
    }
}