using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace MyTools.Services
{
    public class GeometryCenterService
    {
        public static XYZ GetTrueGeometryCenter(Element element)
        {
            // Use Medium/Fine to ensure we get solids, but ignore non-visible junk
            Options opt = new Options
            {
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            GeometryElement geoElem = element.get_Geometry(opt);
            if (geoElem == null) return null;

            double totalVolume = 0;
            XYZ weightedCentroidSum = new XYZ(0, 0, 0);

            foreach (GeometryObject geoObj in geoElem)
            {
                // Look for the main Solid of the element
                if (geoObj is Solid solid && solid.Volume > 0.001)
                {
                    XYZ centroid = solid.ComputeCentroid();
                    weightedCentroidSum += centroid.Multiply(solid.Volume);
                    totalVolume += solid.Volume;
                }
                // Handle Family Instances (Doors, Windows, etc.)
                else if (geoObj is GeometryInstance instance)
                {
                    foreach (GeometryObject instObj in instance.GetInstanceGeometry())
                    {
                        if (instObj is Solid instSolid && instSolid.Volume > 0.001)
                        {
                            XYZ centroid = instSolid.ComputeCentroid();
                            weightedCentroidSum += centroid.Multiply(instSolid.Volume);
                            totalVolume += instSolid.Volume;
                        }
                    }
                }
            }

            // Return the average center weighted by volume
            return totalVolume > 0 ? weightedCentroidSum.Divide(totalVolume) : null;

        }
    }
}