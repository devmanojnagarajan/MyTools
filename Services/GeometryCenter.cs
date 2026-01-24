using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools.Services
{
    /// <summary>
    /// Calculates the geometric center of Revit elements.
    /// </summary>
    public static class GeometryCenter
    {
        /// <summary>
        /// Gets the volume-weighted geometric center of an element.
        /// </summary>
        public static XYZ? GetGeometryCenter(Element element)
        {
            Options opt = new Options
            {
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            GeometryElement geoElem = element.get_Geometry(opt);
            if (geoElem == null)
                return null;

            double totalVolume = 0;
            XYZ weightedCentroidSum = new XYZ(0, 0, 0);

            foreach (GeometryObject geoObj in geoElem)
            {
                if (geoObj is Solid solid && solid.Volume > 0.001)
                {
                    XYZ centroid = solid.ComputeCentroid();
                    weightedCentroidSum += centroid.Multiply(solid.Volume);
                    totalVolume += solid.Volume;
                }
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

            if (totalVolume > 0)
                return weightedCentroidSum.Divide(totalVolume);

            return null;
        }
    }
}
