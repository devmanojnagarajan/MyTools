using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyTools.Services
{
    /// <summary>
    /// Finds TextNotes that fall within region boundaries.
    /// </summary>
    public static class TextNoteClash
    {
        /// <summary>
        /// Returns the first TextNote whose coordinate is inside the given region boundaries.
        /// </summary>
        public static TextNote? GetTextNoteInRegion(IList<CurveLoop>? regionBoundaries, List<TextNote> allNotes)
        {
            if (regionBoundaries == null || regionBoundaries.Count == 0)
                return null;

            foreach (TextNote note in allNotes)
            {
                if (FaceProjectionHelper.IsPointInsideBoundary(regionBoundaries, note.Coord))
                    return note;
            }

            return null;
        }
    }
}
