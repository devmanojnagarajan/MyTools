using Autodesk.Revit.UI;
using System.Linq;
using MyTools.Model;

namespace MyTools.Services
{
    public static class SelectionPrinter
    {
        // Print current selections
        public static void PrintSelections()
        {
            var selectedCategories = CategoriesSelected.GetSelectedCategories();
            var selectedView = ViewSelected.GetSelectedView();

            if (selectedCategories.Count > 0 && selectedView != null)
            {
                string categoryNames = string.Join(", ", selectedCategories.Select(c => c.Name));

                TaskDialog.Show("Create View",
                    $"Creating view based on:\n\n" +
                    $"View: {selectedView.Name}\n\n" +
                    $"Categories ({selectedCategories.Count}):\n{categoryNames}");
            }
            else
            {
                TaskDialog.Show("No Selection", "No categories or view selected.");
            }
        }

        // Print only categories
        public static void PrintCategories()
        {
            var selectedCategories = CategoriesSelected.GetSelectedCategories();

            if (selectedCategories.Count > 0)
            {
                string categoryNames = string.Join(", ", selectedCategories.Select(c => c.Name));
                TaskDialog.Show("Selected Categories",
                    $"Categories ({selectedCategories.Count}):\n{categoryNames}");
            }
            else
            {
                TaskDialog.Show("No Selection", "No categories selected.");
            }
        }

        // Print only view
        public static void PrintView()
        {
            var selectedView = ViewSelected.GetSelectedView();

            if (selectedView != null)
            {
                TaskDialog.Show("Selected View", $"View: {selectedView.Name}");
            }
            else
            {
                TaskDialog.Show("No Selection", "No view selected.");
            }
        }
    }
}