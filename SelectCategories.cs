using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace MyTools
{
    [Transaction(TransactionMode.Manual)]
    public class SelectCategories : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            // Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get all categories in the document
            Categories categories = doc.Settings.Categories;

            // Filter to get only model categories (you can modify this filter)
            List<Category> modelCategories = new List<Category>();
            foreach (Category cat in categories)
            {
                // Only include categories that allow bound parameters (model categories)
                if (cat.AllowsBoundParameters && cat.CategoryType == CategoryType.Model)
                {
                    modelCategories.Add(cat);
                }
            }

            // Sort categories by name
            modelCategories = modelCategories.OrderBy(c => c.Name).ToList();

            // Convert to CategoryItem list for the WPF window
            List<CategoryItem> categoryItems = modelCategories
                .Select(c => new CategoryItem
                {
                    Name = c.Name,
                    CategoryId = c.Id,
                    IsSelected = false
                })
                .ToList();

            // Show the category selection window
            CategorySelectionWindow window = new CategorySelectionWindow(categoryItems);

            // Set Revit as the owner window
            System.Windows.Interop.WindowInteropHelper helper =
                new System.Windows.Interop.WindowInteropHelper(window);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            if (window.ShowDialog() == true)
            {
                // Get selected categories
                List<CategoryItem> selected = window.SelectedCategories;

                if (selected.Count > 0)
                {
                    // TODO: Add your logic here for what to do with selected categories
                    string selectedNames = string.Join("\n", selected.Select(c => c.Name));
                    TaskDialog.Show("Selected Categories", $"You selected {selected.Count} categories:\n\n{selectedNames}");
                }
                else
                {
                    TaskDialog.Show("Selection", "No categories selected.");
                }
            }

            return Result.Succeeded;
        }
    }
}
