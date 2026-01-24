using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using MyTools.Services;

namespace MyTools
{
    /// <summary>
    /// External command that opens the category selection window and generates isolated views.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class SelectCategories : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentActiveView = uidoc.ActiveView;

            if (!ValidateActiveView(currentActiveView))
                return Result.Cancelled;

            ViewItem currentViewItem = CreateViewItem(currentActiveView);
            List<CategoryItem> categoryItems = GetModelCategories(doc);
            List<ViewItem> viewItems = Get3DViews(doc);

            CategorySelectionWindow window = new CategorySelectionWindow(categoryItems, viewItems, currentViewItem);
            SetRevitAsOwner(window);

            if (window.ShowDialog() != true || !window.CreateViewClicked)
                return Result.Succeeded;

            return ProcessSelection(doc, currentActiveView, window);
        }

        private bool ValidateActiveView(View view)
        {
            if (view.ViewType == ViewType.FloorPlan)
                return true;

            TaskDialog.Show("Invalid View Type",
                $"This command must be run from a Floor Plan view.\n\n" +
                $"Current view: {view.Name}\n" +
                $"View type: {view.ViewType}\n\n" +
                "Please switch to a Floor Plan view and try again.");

            return false;
        }

        private ViewItem CreateViewItem(View view)
        {
            return new ViewItem
            {
                Name = view.Name,
                ViewId = view.Id
            };
        }

        private List<CategoryItem> GetModelCategories(Document doc)
        {
            return doc.Settings.Categories
                .Cast<Category>()
                .Where(c => c.AllowsBoundParameters && c.CategoryType == CategoryType.Model)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryItem
                {
                    Name = c.Name,
                    CategoryId = c.Id,
                    IsSelected = false
                })
                .ToList();
        }

        private List<ViewItem> Get3DViews(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted && v.ViewType == ViewType.ThreeD)
                .OrderBy(v => v.Name)
                .Select(v => new ViewItem
                {
                    Name = v.Name,
                    ViewId = v.Id
                })
                .ToList();
        }

        private void SetRevitAsOwner(System.Windows.Window window)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(window);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        }

        private Result ProcessSelection(Document doc, View activeView, CategorySelectionWindow window)
        {
            ViewItem? selectedTemplateView = window.SelectedView;

            if (selectedTemplateView == null)
            {
                TaskDialog.Show("Error", "Please select a 3D View to use as a template.");
                return Result.Failed;
            }

            List<ElementId> categoryIds = window.SelectedCategories
                .Where(c => c.IsSelected)
                .Select(c => c.CategoryId)
                .ToList();

            if (categoryIds.Count == 0)
            {
                TaskDialog.Show("Warning", "No categories were selected. Process cancelled.");
                return Result.Cancelled;
            }

            ViewGenerationService.GenerateViews(doc, activeView, selectedTemplateView.ViewId, categoryIds);
            return Result.Succeeded;
        }
    }
}
