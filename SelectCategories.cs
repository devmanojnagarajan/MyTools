using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyTools.Services;
using MyTools.Model;

namespace MyTools
{
    [Transaction(TransactionMode.Manual)]
    public class SelectCategories : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Debug.WriteLine("=======================================================");
            Debug.WriteLine("=== SelectCategories.Execute START ===");
            Debug.WriteLine("=======================================================");

            // Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Debug.WriteLine($"Document: {doc.Title}");

            // GET THE CURRENT ACTIVE VIEW
            View currentActiveView = uidoc.ActiveView;
            Debug.WriteLine($"Active View: {currentActiveView.Name} (Type: {currentActiveView.ViewType})");

            // CHECK IF CURRENT VIEW IS A FLOOR PLAN
            if (currentActiveView.ViewType != ViewType.FloorPlan)
            {
                Debug.WriteLine("ERROR: Current view is not a FloorPlan. Aborting.");
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
            Debug.WriteLine($"Found {modelCategories.Count} model categories");

            // Convert to CategoryItem list
            List<CategoryItem> categoryItems = modelCategories
                .Select(c => new CategoryItem
                {
                    Name = c.Name,
                    CategoryId = c.Id,
                    IsSelected = false
                })
                .ToList();

            // Get all views in the document (only 3D Views for selection)
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            List<View> allViews = viewCollector
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate
                    && v.CanBePrinted
                    && v.ViewType == ViewType.ThreeD) // Only 3D Views
                .OrderBy(v => v.Name)
                .ToList();
            Debug.WriteLine($"Found {allViews.Count} 3D views for template selection");

            // Convert to ViewItem list
            List<ViewItem> viewItems = allViews
                .Select(v => new ViewItem
                {
                    Name = v.Name,
                    ViewId = v.Id
                })
                .ToList();

            // Show the category selection window with current active view
            Debug.WriteLine("Opening CategorySelectionWindow...");
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
                Debug.WriteLine("User clicked Create View button");

                // 1. Get the User Selection from the Window
                List<CategoryItem> selectedCategories = window.SelectedCategories;
                ViewItem selectedTemplateView = window.SelectedView;

                if (selectedTemplateView == null)
                {
                    Debug.WriteLine("ERROR: No template view selected");
                    TaskDialog.Show("Error", "Please select a 3D View to use as a template.");
                    return Result.Failed;
                }
                Debug.WriteLine($"Selected template view: {selectedTemplateView.Name}");

                // 2. Convert UI Items back to Revit IDs
                ElementId templateId = selectedTemplateView.ViewId;

                List<ElementId> categoryIdsToProcess = selectedCategories
                    .Where(c => c.IsSelected)
                    .Select(c => c.CategoryId)
                    .ToList();

                Debug.WriteLine($"Selected {categoryIdsToProcess.Count} categories:");
                foreach (var cat in selectedCategories.Where(c => c.IsSelected))
                {
                    Debug.WriteLine($"  - {cat.Name}");
                }

                if (categoryIdsToProcess.Count == 0)
                {
                    Debug.WriteLine("WARNING: No categories selected. Aborting.");
                    TaskDialog.Show("Warning", "No categories were selected. Process cancelled.");
                    return Result.Cancelled;
                }

                // 3. CALL THE MAIN FUNCTION TO PROCESS EVERYTHING
                Debug.WriteLine("Calling ViewGenerationService.GenerateViews...");
                ViewGenerationService.GenerateViews(doc, currentActiveView, templateId, categoryIdsToProcess);
            }
            else
            {
                Debug.WriteLine("User cancelled or closed the window");
            }

            Debug.WriteLine("=== SelectCategories.Execute END ===");
            return Result.Succeeded;
        }
    }
}