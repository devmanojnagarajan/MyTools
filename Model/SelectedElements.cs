using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Model
{
    public class ElementsSelect
    {
        public static List<Element> GetModelElementsInView(Document doc, ViewItem currentView)
        {
            List<Element> allModelElementsInView = new FilteredElementCollector(doc, currentView.ViewId)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .Where(e => e.Category != null && e.Category.CategoryType == CategoryType.Model)
                .ToList();

            return allModelElementsInView;
        }
    }
}
