using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace MyTools
{
    public class TextNoteClash
    {
        public static TextNote GetTextNoteInRegion(Face filledRegionFace, List<TextNote> allNotes)
        {
            if (filledRegionFace == null) return null;

            foreach (TextNote note in allNotes)
            {
                // 1. Get the center (insertion) point of the text note
                XYZ notePoint = note.Coord;

                // 2. Project the point onto the face of the filled region
                // Face.Project returns an IntersectionResult if the point can be 
                // projected orthogonally onto the face boundaries.
                IntersectionResult result = filledRegionFace.Project(notePoint);

                // 3. Check if the projection happened and is within tolerance
                // If result is not null, the point is directly "over" or "under" the face area.
                if (result != null && result.Distance < 0.001)
                {
                    // Return the textnote if found
                    return note;
                }
            }

            return null;
        }
    }
}