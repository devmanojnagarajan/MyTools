using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using MyTools.Model;

namespace MyTools.Services
{
    public class ViewGenerationService
    {
        public static void GenerateViews(Document doc, View activePlanView, ElementId templateViewId, List<ElementId> selectedCategoryIds)
        {
            // 1. Get the Template 3D View selected by the user
            View3D? templateView = doc.GetElement(templateViewId) as View3D;
            if (templateView == null)
            {
                TaskDialog.Show("Error", "The selected view is not a valid 3D View.");
                return;
            }

            // 2. Collect Context Elements (Filled Regions & Text Notes) from the Active Plan
            List<FilledRegion> regions = new FilteredElementCollector(doc, activePlanView.Id)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();

            List<TextNote> notes = new FilteredElementCollector(doc, activePlanView.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .ToList();

            // 3. Collect Model Elements to check for clashes
            // We optimize by only collecting elements matching the USER'S SELECTED CATEGORIES
            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(selectedCategoryIds);

            List<Element> candidateElements = new FilteredElementCollector(doc, activePlanView.Id)
                .WherePasses(catFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            int viewsCreated = 0;

            // 4. Start Transaction for View Creation
            using (Transaction trans = new Transaction(doc, "Generate Isolated Views"))
            {
                trans.Start();

                foreach (FilledRegion region in regions)
                {
                    // A. Create Projection Face
                    Face regionFace = CreateSurface.GetFaceFromFilledRegion(region, 0);
                    if (regionFace == null) continue;

                    // B. Find Associated TextNote
                    TextNote labelNote = TextNoteClash.GetTextNoteInRegion(regionFace, notes);
                    if (labelNote == null) continue;

                    // C. Slice Name from Quotes
                    // Assuming you have this method in your StringUtils/ViewNameFromString class
                    string viewName = ViewNameFromString.TitleText(labelNote.Text);
                    if (string.IsNullOrEmpty(viewName)) continue;

                    // D. Check Clashes using your Face Projection Logic
                    List<Element> clashingElements = CheckClash.getClashingElements(regionFace, candidateElements);

                    if (clashingElements.Count > 0)
                    {
                        // E. Create the View using your IsolatedView Logic
                        CreateNewIsolated3DView.ViewCreate(templateView, clashingElements, viewName);
                        viewsCreated++;
                    }
                }

                trans.Commit();
            }

            TaskDialog.Show("Process Complete", $"Successfully created {viewsCreated} views.");
        }
    }
}