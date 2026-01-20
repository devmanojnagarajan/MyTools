using Autodesk.Revit.DB;
using System.Collections.Generic;
using MyTools.Services;

namespace MyTools.Model
{
    public static class CategoriesSelected
    {
        private static List<CategoryItem> _selectedCategories = new List<CategoryItem>();
       

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

        // Clear all selections
        public static void ClearSelections()
        {
            _selectedCategories.Clear();
            
        }

        // Check if selections exist
        public static bool HasSelections()
        {
            return _selectedCategories.Count > 0;
        }
    }
}