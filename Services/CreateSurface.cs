using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools.Services
{
    public class CreateSurface
    {
        public static Face GetFaceFromFilledRegion(FilledRegion filledRegion, double offsetDistance)
        {
            IList<CurveLoop> currentLoop = filledRegion.GetBoundaries();
            if (currentLoop.Count == 0)
            {
                Console.WriteLine("Boundary Loop is not valid");
                return null;
            }
            

            Plane regionPlane = currentLoop[0].GetPlane();
            XYZ normal = regionPlane.Normal;

            Transform moveBack = Transform.CreateTranslation(normal.Multiply(-offsetDistance));

            IList<CurveLoop> startProfile = new List<CurveLoop>();

            foreach (CurveLoop loop in currentLoop)
            {
                startProfile.Add(CurveLoop.CreateViaTransform(loop, moveBack));
            }

            Solid tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(startProfile, normal, 0.1 * offsetDistance);

            return tempSolid.Faces.get_Item(0) as Face;

        }

    }
}
