using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools.Model
{
    public class FilledRegionsInView 

    {
        private static List<FilledRegion> filledRegionsInView = new List<FilledRegion>();

        //get all the filled region in the view
        public static void selectAllFilledRegionsFromView(View currentView, Document doc)
        {
            filledRegionsInView.Clear();

            FilteredElementCollector collector = new FilteredElementCollector(doc, currentView.Id);

            filledRegionsInView = collector
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();


        }

        public static List<FilledRegion> GetFilledRegions()
        {
            return filledRegionsInView;
        }

        public static int getFilledRegionsCount()
        {
            return filledRegionsInView.Count;
        }

        public static void clearFilledRegionsList()
        {
            filledRegionsInView.Clear();
        }




    }
    
}
