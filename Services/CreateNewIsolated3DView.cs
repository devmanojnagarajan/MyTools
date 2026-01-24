using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Services
{
    /// <summary>
    /// Creates isolated 3D views from a template view.
    /// </summary>
    public static class CreateNewIsolated3DView
    {
        /// <summary>
        /// Creates a new 3D view by duplicating a template and isolating specified elements.
        /// </summary>
        public static void ViewCreate(View3D templateView, List<Element> elementsToIsolate, string newViewName)
        {
            Document doc = templateView.Document;

            ElementId newViewId = templateView.Duplicate(ViewDuplicateOption.Duplicate);
            View3D? isolatedView = doc.GetElement(newViewId) as View3D;

            if (isolatedView == null)
                return;

            SetViewName(isolatedView, newViewName);
            HideNonTargetElements(doc, isolatedView, elementsToIsolate);
            SetSectionBox(isolatedView, elementsToIsolate);
        }

        private static void SetViewName(View3D view, string desiredName)
        {
            try
            {
                view.Name = desiredName;
            }
            catch
            {
                string fallbackName = desiredName + "_" + Guid.NewGuid().ToString().Substring(0, 4);
                view.Name = fallbackName;
            }
        }

        private static void HideNonTargetElements(Document doc, View3D view, List<Element> elementsToKeep)
        {
            ICollection<ElementId> allElementIdsInView = new FilteredElementCollector(doc, view.Id)
                .WhereElementIsNotElementType()
                .ToElementIds();

            HashSet<ElementId> targetIds = new HashSet<ElementId>(elementsToKeep.Select(e => e.Id));

            List<ElementId> idsToHide = new List<ElementId>();

            foreach (ElementId id in allElementIdsInView)
            {
                if (targetIds.Contains(id))
                    continue;

                Element? el = doc.GetElement(id);
                if (el != null && el.CanBeHidden(view))
                    idsToHide.Add(id);
            }

            if (idsToHide.Count > 0)
                view.HideElements(idsToHide);
        }

        private static void SetSectionBox(View3D view, List<Element> elements)
        {
            BoundingBoxXYZ? boundingBox = GetElementsBoundingBox(elements);
            if (boundingBox != null)
                view.SetSectionBox(boundingBox);
        }

        private static BoundingBoxXYZ? GetElementsBoundingBox(List<Element> elements)
        {
            if (elements.Count == 0)
                return null;

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            bool hasValidBbox = false;

            foreach (Element el in elements)
            {
                BoundingBoxXYZ? bbox = el.get_BoundingBox(null);
                if (bbox == null)
                    continue;

                hasValidBbox = true;
                minX = Math.Min(minX, bbox.Min.X);
                minY = Math.Min(minY, bbox.Min.Y);
                minZ = Math.Min(minZ, bbox.Min.Z);
                maxX = Math.Max(maxX, bbox.Max.X);
                maxY = Math.Max(maxY, bbox.Max.Y);
                maxZ = Math.Max(maxZ, bbox.Max.Z);
            }

            if (!hasValidBbox)
                return null;

            return new BoundingBoxXYZ
            {
                Min = new XYZ(minX, minY, minZ),
                Max = new XYZ(maxX, maxY, maxZ)
            };
        }
    }
}
