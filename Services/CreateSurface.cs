using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools
{
    public class CreateVolume
    {

        public static Solid SolidFromFilledRegion(FilledRegion filledRegion, double extrudeDistance)
        {
            IList<CurveLoop> currentLoop = filledRegion.GetBoundaries();

            /*
            if (currentLoop.Count == 0)
            {
                return null;
            }
            */

            Plane regionPlane = currentLoop[0].GetPlane():
            XYZ normal = regionPlane.Normal;

            Transform moveBack = Transform.CreateTranslation(normal.Multiply(-extrudeDistance));

            IList<CurveLoop> startProfile = new List<CurveLoop>();

            foreach (CurveLoop loop in currentLoop)
            {
                startProfile.Add(CurveLoop.CreateViaTransform(loop, moveBack));
            }

            Solid intersectionVolume = GeometryCreationUtilities.CreateExtrusionGeometry(startProfile, normal, 2 * extrudeDistance);

            return intersectionVolume;

        }

    }
}
