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

            // Filter to get only model categories
            List<Category> modelCategories = new List<Category>();
            foreach (Category cat in categories)
            {
                if (cat.AllowsBoundParameters && cat.CategoryType == CategoryType.Model)
                {
                    modelCategories.Add(cat);
                }
            }

            // Sort categories by name
            modelCategories = modelCategories.OrderBy(c => c.Name).ToList();

            // Convert to CategoryItem list
            List<CategoryItem> categoryItems = modelCategories
                .Select(c => new CategoryItem
                {
                    Name = c.Name,
                    CategoryId = c.Id,
                    IsSelected = false
                })
                .ToList();

            // Get all views in the document
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            List<View> allViews = viewCollector
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted)
                .OrderBy(v => v.Name)
                .ToList();

            // Convert to ViewItem list
            List<ViewItem> viewItems = allViews
                .Select(v => new ViewItem
                {
                    Name = v.Name,
                    ViewId = v.Id
                })
                .ToList();

            // Show the category selection window
            CategorySelectionWindow window = new CategorySelectionWindow(categoryItems, viewItems);

            // Set Revit as the owner window
            System.Windows.Interop.WindowInteropHelper helper =
                new System.Windows.Interop.WindowInteropHelper(window);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            if (window.ShowDialog() == true && window.CreateViewClicked)
            {
                List<CategoryItem> selectedCategories = window.SelectedCategories;
                ViewItem selectedView = window.SelectedView;

                // Store selections using SelectionStorage
                SelectionStorage.SetSelectedCategories(selectedCategories);
                SelectionStorage.SetSelectedView(selectedView);

                // Print selections using SelectionPrinter
                SelectionPrinter.PrintSelections();
            }

            return Result.Succeeded;
        }
    }
}