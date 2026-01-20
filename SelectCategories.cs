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

            // GET THE CURRENT ACTIVE VIEW
            View currentActiveView = uidoc.ActiveView;

            // CHECK IF CURRENT VIEW IS A FLOOR PLAN
            if (currentActiveView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Invalid View Type",
                    "This command must be run from a Floor Plan view.\n\n" +
                    $"Current view: {currentActiveView.Name}\n" +
                    $"View type: {currentActiveView.ViewType}\n\n" +
                    "Please switch to a Floor Plan view and try again.");
                return Result.Cancelled;
            }

            // Create ViewItem for current active view
            ViewItem currentViewItem = new ViewItem
            {
                Name = currentActiveView.Name,
                ViewId = currentActiveView.Id
            };

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

            // Get all views in the document (only floor plans for selection)
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            List<View> allViews = viewCollector
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate
                    && v.CanBePrinted
                    && v.ViewType == ViewType.FloorPlan) // Only floor plans
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

            // Show the category selection window with current active view
            CategorySelectionWindow window = new CategorySelectionWindow(
                categoryItems,
                viewItems,
                currentViewItem); // Pass the current active view

            // Set Revit as the owner window
            System.Windows.Interop.WindowInteropHelper helper =
                new System.Windows.Interop.WindowInteropHelper(window);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            if (window.ShowDialog() == true && window.CreateViewClicked)
            {
                // 1. Get the User Selection from the Window
                List<CategoryItem> selectedCategories = window.SelectedCategories;
                ViewItem selectedTemplateView = window.SelectedView;

                if (selectedTemplateView == null)
                {
                    TaskDialog.Show("Error", "Please select a 3D View to use as a template.");
                    return Result.Failed;
                }

                // 2. Convert UI Items back to Revit IDs
                ElementId templateId = selectedTemplateView.ViewId;

                List<ElementId> categoryIdsToProcess = selectedCategories
                    .Where(c => c.IsSelected)
                    .Select(c => c.CategoryId)
                    .ToList();

                if (categoryIdsToProcess.Count == 0)
                {
                    TaskDialog.Show("Warning", "No categories were selected. Process cancelled.");
                    return Result.Cancelled;
                }

                // 3. CALL THE MAIN FUNCTION TO PROCESS EVERYTHING
                ViewGenerationService.GenerateViews(doc, currentActiveView, templateId, categoryIdsToProcess);
            }

            return Result.Succeeded;
        }
    }
}