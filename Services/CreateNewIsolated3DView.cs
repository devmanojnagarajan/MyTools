using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyTools.Model;

namespace MyTools.Services
{
    public class CreateNewIsolated3DView
    {
        public static void ViewCreate(View3D defaultView, List<Element> elementsToIsolate, string newViewName)
        {
            Debug.WriteLine($"      [CreateNewIsolated3DView] ViewCreate START - viewName: '{newViewName}', elements: {elementsToIsolate.Count}");

            Document doc = defaultView.Document;

            // 1. Duplicate the chosen default 3D view
            Debug.WriteLine($"      [CreateNewIsolated3DView] Duplicating template view '{defaultView.Name}'...");
            ElementId newViewId = defaultView.Duplicate(ViewDuplicateOption.Duplicate);
            View3D isolatedView = doc.GetElement(newViewId) as View3D;
            Debug.WriteLine($"      [CreateNewIsolated3DView] Duplicated view Id: {newViewId}");

            // 2. Set the Name extracted from your TextNote quotes
            try
            {
                isolatedView.Name = newViewName;
                Debug.WriteLine($"      [CreateNewIsolated3DView] View name set to: '{newViewName}'");
            }
            catch (Exception ex)
            {
                string fallbackName = newViewName + "_" + Guid.NewGuid().ToString().Substring(0, 4);
                isolatedView.Name = fallbackName;
                Debug.WriteLine($"      [CreateNewIsolated3DView] Name conflict, using fallback: '{fallbackName}' (Error: {ex.Message})");
            }

            // 3. Perform Permanent Isolation
            // We need to find all elements in the new view and hide those that aren't in our list
            ICollection<ElementId> allElementIdsInView = new FilteredElementCollector(doc, isolatedView.Id)
                .WhereElementIsNotElementType()
                .ToElementIds();
            Debug.WriteLine($"      [CreateNewIsolated3DView] Total elements in view: {allElementIdsInView.Count}");

            List<ElementId> targetIds = elementsToIsolate.Select(e => e.Id).ToList();
            Debug.WriteLine($"      [CreateNewIsolated3DView] Elements to keep visible: {targetIds.Count}");

            // Filter out the IDs we want to KEEP, and only include elements that CAN be hidden
            List<ElementId> idsToHide = new List<ElementId>();
            int cannotHideCount = 0;

            foreach (ElementId id in allElementIdsInView)
            {
                // Skip elements we want to keep
                if (targetIds.Contains(id))
                    continue;

                // Check if element can be hidden in this view
                Element el = doc.GetElement(id);
                if (el != null && el.CanBeHidden(isolatedView))
                {
                    idsToHide.Add(id);
                }
                else
                {
                    cannotHideCount++;
                }
            }

            Debug.WriteLine($"      [CreateNewIsolated3DView] Elements to hide: {idsToHide.Count}, Cannot hide: {cannotHideCount}");

            if (idsToHide.Any())
            {
                // HideElements is permanent (Visibility/Graphics)
                isolatedView.HideElements(idsToHide);
                Debug.WriteLine($"      [CreateNewIsolated3DView] Hidden {idsToHide.Count} elements");
            }

            // 4. Set Section Box to frame the isolated elements
            BoundingBoxXYZ selectionBox = GetElementsBoundingBox(elementsToIsolate);
            if (selectionBox != null)
            {
                isolatedView.SetSectionBox(selectionBox);
                Debug.WriteLine($"      [CreateNewIsolated3DView] Section box set - Min: ({selectionBox.Min.X:F2}, {selectionBox.Min.Y:F2}, {selectionBox.Min.Z:F2}), Max: ({selectionBox.Max.X:F2}, {selectionBox.Max.Y:F2}, {selectionBox.Max.Z:F2})");
            }
            else
            {
                Debug.WriteLine($"      [CreateNewIsolated3DView] WARNING: Could not create section box");
            }

            Debug.WriteLine($"      [CreateNewIsolated3DView] ViewCreate END - View '{isolatedView.Name}' created successfully");
        }

        private static BoundingBoxXYZ GetElementsBoundingBox(List<Element> elements)
        {
            Debug.WriteLine($"      [CreateNewIsolated3DView] GetElementsBoundingBox - processing {elements.Count} elements");

            if (elements.Count == 0)
            {
                Debug.WriteLine($"      [CreateNewIsolated3DView] GetElementsBoundingBox - No elements, returning null");
                return null;
            }

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            int validBboxCount = 0;
            foreach (Element el in elements)
            {
                BoundingBoxXYZ bbox = el.get_BoundingBox(null);
                if (bbox == null) continue;

                validBboxCount++;
                minX = Math.Min(minX, bbox.Min.X);
                minY = Math.Min(minY, bbox.Min.Y);
                minZ = Math.Min(minZ, bbox.Min.Z);

                maxX = Math.Max(maxX, bbox.Max.X);
                maxY = Math.Max(maxY, bbox.Max.Y);
                maxZ = Math.Max(maxZ, bbox.Max.Z);
            }

            Debug.WriteLine($"      [CreateNewIsolated3DView] GetElementsBoundingBox - {validBboxCount}/{elements.Count} elements had valid bounding boxes");

            return new BoundingBoxXYZ { Min = new XYZ(minX, minY, minZ), Max = new XYZ(maxX, maxY, maxZ) };
        }
    }
}