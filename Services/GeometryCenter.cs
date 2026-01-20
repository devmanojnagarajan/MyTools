using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyTools.Model;

namespace MyTools.Services
{
    public class GeometryCenter
    {
        public static XYZ GetGeometryCenter(Element element)
        {
            // Use Medium/Fine to ensure we get solids, but ignore non-visible junk
            Options opt = new Options
            {
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            GeometryElement geoElem = element.get_Geometry(opt);
            if (geoElem == null)
            {
                Debug.WriteLine($"      [GeometryCenter] Element {element.Id} has no geometry");
                return null;
            }

            double totalVolume = 0;
            XYZ weightedCentroidSum = new XYZ(0, 0, 0);
            int solidCount = 0;

            foreach (GeometryObject geoObj in geoElem)
            {
                // Look for the main Solid of the element
                if (geoObj is Solid solid && solid.Volume > 0.001)
                {
                    XYZ centroid = solid.ComputeCentroid();
                    weightedCentroidSum += centroid.Multiply(solid.Volume);
                    totalVolume += solid.Volume;
                    solidCount++;
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
                            solidCount++;
                        }
                    }
                }
            }

            // Return the average center weighted by volume
            if (totalVolume > 0)
            {
                XYZ result = weightedCentroidSum.Divide(totalVolume);
                Debug.WriteLine($"      [GeometryCenter] Element {element.Id}: {solidCount} solids, volume={totalVolume:F3}, center=({result.X:F2}, {result.Y:F2}, {result.Z:F2})");
                return result;
            }

            Debug.WriteLine($"      [GeometryCenter] Element {element.Id}: No valid solids found (totalVolume=0)");
            return null;
        }
    }
}