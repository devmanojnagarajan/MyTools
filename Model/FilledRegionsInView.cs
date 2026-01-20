using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Model
{
    public class FilledRegionsInView
    {
        private static List<FilledRegion> filledRegionsInView = new List<FilledRegion>();

        public static void SelectAllFilledRegionsFromView(View currentView, Document doc)
        {
            filledRegionsInView.Clear();

            filledRegionsInView = new FilteredElementCollector(doc, currentView.Id)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();
        }

        public static List<FilledRegion> GetFilledRegions()
        {
            return filledRegionsInView;
        }

        public static int GetFilledRegionsCount()
        {
            return filledRegionsInView.Count;
        }

        public static void ClearFilledRegionsList()
        {
            filledRegionsInView.Clear();
        }
    }
}
