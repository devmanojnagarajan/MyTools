using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools.Services
{
    /// <summary>
    /// Extracts boundary curves from FilledRegion elements.
    /// </summary>
    public static class CreateSurface
    {
        /// <summary>
        /// Gets the boundary curves of a FilledRegion as a list of CurveLoops.
        /// </summary>
        public static IList<CurveLoop>? GetBoundaryLoops(FilledRegion filledRegion)
        {
            IList<CurveLoop> boundaries = filledRegion.GetBoundaries();

            if (boundaries == null || boundaries.Count == 0)
                return null;

            return boundaries;
        }
    }
}
