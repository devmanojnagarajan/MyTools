using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Services
{
    /// <summary>
    /// Generates isolated 3D views from filled regions in a floor plan.
    /// </summary>
    public static class ViewGenerationService
    {
        /// <summary>
        /// Generates corridor and room views based on filled regions and their associated text notes.
        /// </summary>
        public static void GenerateViews(Document doc, View activePlanView, ElementId templateViewId, List<ElementId> selectedCategoryIds)
        {
            View3D? templateView = doc.GetElement(templateViewId) as View3D;
            if (templateView == null)
            {
                TaskDialog.Show("Error", "The selected view is not a valid 3D View.");
                return;
            }

            List<FilledRegion> regions = CollectFilledRegions(doc, activePlanView);
            List<TextNote> notes = CollectTextNotes(doc, activePlanView);
            List<Element> candidateElements = CollectCandidateElements(doc, activePlanView, selectedCategoryIds);

            var result = ProcessRegions(regions, notes, candidateElements);

            using (Transaction trans = new Transaction(doc, "Generate Isolated Views"))
            {
                trans.Start();

                int viewsCreated = 0;

                // Create Corridor view with elements NOT in any room
                List<Element> corridorElements = candidateElements
                    .Where(e => !result.ElementsInRooms.Contains(e.Id))
                    .ToList();

                if (corridorElements.Count > 0)
                {
                    string corridorViewName = "Corridor " + activePlanView.Name;
                    CreateNewIsolated3DView.ViewCreate(templateView, corridorElements, corridorViewName);
                    viewsCreated++;
                }

                // Create individual room views
                foreach (var regionData in result.RegionViewData)
                {
                    CreateNewIsolated3DView.ViewCreate(templateView, regionData.Elements, regionData.ViewName);
                    viewsCreated++;
                }

                trans.Commit();

                ShowResultDialog(regions.Count, notes.Count, candidateElements.Count, result, viewsCreated);
            }
        }

        private static List<FilledRegion> CollectFilledRegions(Document doc, View view)
        {
            return new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();
        }

        private static List<TextNote> CollectTextNotes(Document doc, View view)
        {
            return new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .ToList();
        }

        private static List<Element> CollectCandidateElements(Document doc, View view, List<ElementId> categoryIds)
        {
            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(categoryIds);

            return new FilteredElementCollector(doc, view.Id)
                .WherePasses(catFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        private static ProcessingResult ProcessRegions(List<FilledRegion> regions, List<TextNote> notes, List<Element> candidateElements)
        {
            var result = new ProcessingResult();

            foreach (FilledRegion region in regions)
            {
                IList<CurveLoop>? boundaries = CreateSurface.GetBoundaryLoops(region);
                if (boundaries == null)
                {
                    result.SkipNullBoundary++;
                    continue;
                }

                TextNote? labelNote = TextNoteClash.GetTextNoteInRegion(boundaries, notes);
                if (labelNote == null)
                {
                    result.SkipNoTextNote++;
                    continue;
                }

                string viewName = ViewNameFromString.TitleText(labelNote.Text);
                if (string.IsNullOrEmpty(viewName))
                {
                    result.SkipNoQuotedName++;
                    continue;
                }

                List<Element> clashingElements = CheckClash.GetClashingElements(boundaries, candidateElements);

                if (clashingElements.Count > 0)
                {
                    foreach (Element el in clashingElements)
                        result.ElementsInRooms.Add(el.Id);

                    result.RegionViewData.Add(new RegionViewData(clashingElements, viewName));
                }
                else
                {
                    result.SkipNoClash++;
                }
            }

            return result;
        }

        private static void ShowResultDialog(int regionsCount, int notesCount, int elementsCount, ProcessingResult result, int viewsCreated)
        {
            string message = $"Process Complete\n\n" +
                $"Filled Regions: {regionsCount}\n" +
                $"Text Notes: {notesCount}\n" +
                $"Candidate Elements: {elementsCount}\n\n" +
                $"Views Created: {viewsCreated}\n\n" +
                $"Skipped:\n" +
                $"  No boundary: {result.SkipNullBoundary}\n" +
                $"  No text note: {result.SkipNoTextNote}\n" +
                $"  No quoted name: {result.SkipNoQuotedName}\n" +
                $"  No elements: {result.SkipNoClash}";

            TaskDialog.Show("View Generation", message);
        }

        private class ProcessingResult
        {
            public HashSet<ElementId> ElementsInRooms { get; } = new HashSet<ElementId>();
            public List<RegionViewData> RegionViewData { get; } = new List<RegionViewData>();
            public int SkipNullBoundary { get; set; }
            public int SkipNoTextNote { get; set; }
            public int SkipNoQuotedName { get; set; }
            public int SkipNoClash { get; set; }
        }

        private class RegionViewData
        {
            public List<Element> Elements { get; }
            public string ViewName { get; }

            public RegionViewData(List<Element> elements, string viewName)
            {
                Elements = elements;
                ViewName = viewName;
            }
        }
    }
}
