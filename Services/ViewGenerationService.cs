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
            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(selectedCategoryIds);

            List<Element> candidateElements = new FilteredElementCollector(doc, activePlanView.Id)
                .WherePasses(catFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
            Debug.WriteLine($"  Found {candidateElements.Count} candidate elements matching selected categories");

            int viewsCreated = 0;
            int regionIndex = 0;

            // Diagnostic counters
            int skipNullBoundary = 0;
            int skipNoTextNote = 0;
            int skipNoQuotedName = 0;
            int skipNoClash = 0;
            List<string> textNotesFound = new List<string>();
            List<string> viewNamesExtracted = new List<string>();

            // 4. Start Transaction for View Creation
            using (Transaction trans = new Transaction(doc, "Generate Isolated Views"))
            {
                trans.Start();
                Debug.WriteLine("  Transaction started");

                foreach (FilledRegion region in regions)
                {
                    regionIndex++;
                    Debug.WriteLine($"  --- Processing FilledRegion {regionIndex}/{regions.Count} (Id: {region.Id}) ---");

                    // A. Get Boundary Curves from FilledRegion (simple 2D approach)
                    IList<CurveLoop> regionBoundaries = CreateSurface.GetBoundaryLoops(region);
                    if (regionBoundaries == null || regionBoundaries.Count == 0)
                    {
                        Debug.WriteLine($"    SKIP: regionBoundaries is null or empty");
                        skipNullBoundary++;
                        continue;
                    }
                    Debug.WriteLine($"    Boundaries retrieved successfully");

                    // B. Find Associated TextNote (2D point-in-polygon check)
                    TextNote labelNote = TextNoteClash.GetTextNoteInRegion(regionBoundaries, notes);
                    if (labelNote == null)
                    {
                        Debug.WriteLine($"    SKIP: No TextNote found in region");
                        skipNoTextNote++;
                        continue;
                    }
                    Debug.WriteLine($"    Found TextNote: '{labelNote.Text}'");
                    textNotesFound.Add(labelNote.Text);

                    // C. Extract view name from quoted text
                    string viewName = ViewNameFromString.TitleText(labelNote.Text);
                    if (string.IsNullOrEmpty(viewName))
                    {
                        Debug.WriteLine($"    SKIP: viewName is empty (no quoted text found)");
                        skipNoQuotedName++;
                        continue;
                    }
                    Debug.WriteLine($"    Extracted viewName: '{viewName}'");
                    viewNamesExtracted.Add(viewName);

                    // D. Check which elements are inside the region boundary
                    List<Element> clashingElements = CheckClash.GetClashingElements(regionBoundaries, candidateElements);
                    Debug.WriteLine($"    Found {clashingElements.Count} clashing elements");

                    if (clashingElements.Count > 0)
                    {
                        // E. Create the View
                        Debug.WriteLine($"    Creating isolated view '{viewName}'...");
                        CreateNewIsolated3DView.ViewCreate(templateView, clashingElements, viewName);
                        viewsCreated++;
                        Debug.WriteLine($"    View created successfully. Total views: {viewsCreated}");
                    }
                    else
                    {
                        Debug.WriteLine($"    SKIP: No clashing elements found");
                        skipNoClash++;
                    }
                }

                trans.Commit();
                Debug.WriteLine("  Transaction committed");
            }

            Debug.WriteLine($"=== ViewGenerationService.GenerateViews END - Created {viewsCreated} views ===");

            // Build diagnostic message
            string diagnosticMsg = $"=== DIAGNOSTIC REPORT ===\n\n" +
                $"FilledRegions (hatches) found: {regions.Count}\n" +
                $"TextNotes in view: {notes.Count}\n" +
                $"Elements in selected categories: {candidateElements.Count}\n\n" +
                $"=== SKIP REASONS ===\n" +
                $"Boundary retrieval failed: {skipNullBoundary}\n" +
                $"No TextNote in region: {skipNoTextNote}\n" +
                $"No quoted name in text: {skipNoQuotedName}\n" +
                $"No clashing elements: {skipNoClash}\n\n" +
                $"=== TEXT NOTES FOUND IN REGIONS ===\n" +
                (textNotesFound.Count > 0 ? string.Join("\n", textNotesFound) : "(none)") + "\n\n" +
                $"=== VIEW NAMES EXTRACTED ===\n" +
                (viewNamesExtracted.Count > 0 ? string.Join("\n", viewNamesExtracted) : "(none)") + "\n\n" +
                $"=== RESULT ===\n" +
                $"Views created: {viewsCreated}";

            TaskDialog.Show("Process Complete", diagnosticMsg);
        }
    }
}
