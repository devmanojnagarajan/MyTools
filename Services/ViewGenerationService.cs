using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyTools.Model;

namespace MyTools.Services
{
    public class ViewGenerationService
    {
        public static void GenerateViews(Document doc, View activePlanView, ElementId templateViewId, List<ElementId> selectedCategoryIds)
        {
            Debug.WriteLine("=== ViewGenerationService.GenerateViews START ===");
            Debug.WriteLine($"  Active Plan View: {activePlanView.Name}");
            Debug.WriteLine($"  Template View Id: {templateViewId}");
            Debug.WriteLine($"  Selected Category Count: {selectedCategoryIds.Count}");

            // 1. Get the Template 3D View selected by the user
            View3D? templateView = doc.GetElement(templateViewId) as View3D;
            if (templateView == null)
            {
                Debug.WriteLine("  ERROR: Template view is not a valid 3D View");
                TaskDialog.Show("Error", "The selected view is not a valid 3D View.");
                return;
            }
            Debug.WriteLine($"  Template 3D View: {templateView.Name}");

            // 2. Collect Context Elements (Filled Regions & Text Notes) from the Active Plan
            List<FilledRegion> regions = new FilteredElementCollector(doc, activePlanView.Id)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();
            Debug.WriteLine($"  Found {regions.Count} FilledRegions in view");

            List<TextNote> notes = new FilteredElementCollector(doc, activePlanView.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .ToList();
            Debug.WriteLine($"  Found {notes.Count} TextNotes in view");

            // 3. Collect Model Elements to check for clashes
            // We optimize by only collecting elements matching the USER'S SELECTED CATEGORIES
            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(selectedCategoryIds);

            List<Element> candidateElements = new FilteredElementCollector(doc, activePlanView.Id)
                .WherePasses(catFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
            Debug.WriteLine($"  Found {candidateElements.Count} candidate elements matching selected categories");

            int viewsCreated = 0;
            int regionIndex = 0;

            // 4. Start Transaction for View Creation
            using (Transaction trans = new Transaction(doc, "Generate Isolated Views"))
            {
                trans.Start();
                Debug.WriteLine("  Transaction started");

                foreach (FilledRegion region in regions)
                {
                    regionIndex++;
                    Debug.WriteLine($"  --- Processing FilledRegion {regionIndex}/{regions.Count} (Id: {region.Id}) ---");

                    // A. Create Projection Face
                    Face regionFace = CreateSurface.GetFaceFromFilledRegion(region, 1.0); // Use 1.0 feet offset
                    if (regionFace == null)
                    {
                        Debug.WriteLine($"    SKIP: regionFace is null");
                        continue;
                    }
                    Debug.WriteLine($"    regionFace created successfully");

                    // B. Find Associated TextNote
                    TextNote labelNote = TextNoteClash.GetTextNoteInRegion(regionFace, notes);
                    if (labelNote == null)
                    {
                        Debug.WriteLine($"    SKIP: No TextNote found in region");
                        continue;
                    }
                    Debug.WriteLine($"    Found TextNote: '{labelNote.Text}'");

                    // C. Slice Name from Quotes
                    // Assuming you have this method in your StringUtils/ViewNameFromString class
                    string viewName = ViewNameFromString.TitleText(labelNote.Text);
                    if (string.IsNullOrEmpty(viewName))
                    {
                        Debug.WriteLine($"    SKIP: viewName is empty (no quoted text found)");
                        continue;
                    }
                    Debug.WriteLine($"    Extracted viewName: '{viewName}'");

                    // D. Check Clashes using your Face Projection Logic
                    List<Element> clashingElements = CheckClash.getClashingElements(regionFace, candidateElements);
                    Debug.WriteLine($"    Found {clashingElements.Count} clashing elements");

                    if (clashingElements.Count > 0)
                    {
                        // E. Create the View using your IsolatedView Logic
                        Debug.WriteLine($"    Creating isolated view '{viewName}'...");
                        CreateNewIsolated3DView.ViewCreate(templateView, clashingElements, viewName);
                        viewsCreated++;
                        Debug.WriteLine($"    View created successfully. Total views: {viewsCreated}");
                    }
                    else
                    {
                        Debug.WriteLine($"    SKIP: No clashing elements found");
                    }
                }

                trans.Commit();
                Debug.WriteLine("  Transaction committed");
            }

            Debug.WriteLine($"=== ViewGenerationService.GenerateViews END - Created {viewsCreated} views ===");
            TaskDialog.Show("Process Complete", $"Successfully created {viewsCreated} views.");
        }
    }
}