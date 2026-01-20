using Autodesk.Revit.DB;
using System.Diagnostics;

namespace MyTools.Services
{
    public static class FaceProjectionHelper
    {
        private const double ProjectionTolerance = 0.001;

        /// <summary>
        /// Checks if a point projects onto the face within tolerance.
        /// </summary>
        public static bool IsPointOnFace(Face face, XYZ point)
        {
            if (face == null || point == null)
            {
                Debug.WriteLine($"      [FaceProjectionHelper] IsPointOnFace - face or point is null");
                return false;
            }

            IntersectionResult result = face.Project(point);
            bool isOnFace = result != null && result.Distance < ProjectionTolerance;

            if (isOnFace)
            {
                Debug.WriteLine($"      [FaceProjectionHelper] Point ({point.X:F2}, {point.Y:F2}, {point.Z:F2}) IS on face (distance: {result.Distance:F4})");
            }

            return isOnFace;
        }
    }
}
