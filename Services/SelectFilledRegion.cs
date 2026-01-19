using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools
{
    [Transaction(TransactionMode.Manual)]
    public class SelectFilledRegion : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            // Implementation for selecting filled regions goes here
            return Result.Succeeded;
        }
    }
    
}
