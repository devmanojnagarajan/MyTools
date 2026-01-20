using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Services
{
    public class CreateNewIsolated3DView
    {
        public static void ViewCreate(View3D defaultView, List<Element> elementsToIsolate, string newViewName)
        {
            Document doc = defaultView.Document;

            // 1. Duplicate the chosen default 3D view
            ElementId newViewId = defaultView.Duplicate(ViewDuplicateOption.Duplicate);
            View3D isolatedView = doc.GetElement(newViewId) as View3D;

            // 2. Set the Name extracted from your TextNote quotes
            try
            {
                isolatedView.Name = newViewName;
            }
            catch
            {
                isolatedView.Name = newViewName + "_" + Guid.NewGuid().ToString().Substring(0, 4);
            }

            // 3. Perform Permanent Isolation
            // We need to find all elements in the new view and hide those that aren't in our list
            ICollection<ElementId> allElementIdsInView = new FilteredElementCollector(doc, isolatedView.Id)
                .WhereElementIsNotElementType()
                .ToElementIds();

            List<ElementId> targetIds = elementsToIsolate.Select(e => e.Id).ToList();

            // Filter out the IDs we want to KEEP
            List<ElementId> idsToHide = allElementIdsInView
                .Where(id => !targetIds.Contains(id))
                .ToList();

            if (idsToHide.Any())
            {
                // HideElements is permanent (Visibility/Graphics)
                isolatedView.HideElements(idsToHide);
            }

            // 4. Set Section Box to frame the isolated elements
            BoundingBoxXYZ selectionBox = GetElementsBoundingBox(elementsToIsolate);
            if (selectionBox != null)
            {
                isolatedView.SetSectionBox(selectionBox);
            }
        }

        private static BoundingBoxXYZ GetElementsBoundingBox(List<Element> elements)
        {
            if (elements.Count == 0) return null;

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (Element el in elements)
            {
                BoundingBoxXYZ bbox = el.get_BoundingBox(null);
                if (bbox == null) continue;

                minX = Math.Min(minX, bbox.Min.X);
                minY = Math.Min(minY, bbox.Min.Y);
                minZ = Math.Min(minZ, bbox.Min.Z);

                maxX = Math.Max(maxX, bbox.Max.X);
                maxY = Math.Max(maxY, bbox.Max.Y);
                maxZ = Math.Max(maxZ, bbox.Max.Z);
            }

            return new BoundingBoxXYZ { Min = new XYZ(minX, minY, minZ), Max = new XYZ(maxX, maxY, maxZ) };
        }
    }
}