using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools
{
       
    public class ElementsSelect
    {
        public static List<Element> GetModelElementsInView(Document doc, ViewItem currentView)
        {
            

            var collector = new FilteredElementCollector(doc, currentView.ViewId)
                .WhereElementIsNotElementType()
                .ToElements();

            List<Element> allModelElementsInView = new FilteredElementCollector(doc, currentView.ViewId)
                .WhereElementIsNotElementType()    // Filter 1
                .WhereElementIsViewIndependent()   // Filter 2
                                                   // Generic LINQ Filter (for Categories)
                .Where(e => e.Category != null && e.Category.CategoryType == CategoryType.Model)
                .ToList();


            return allModelElementsInView;

        }


    }
}
