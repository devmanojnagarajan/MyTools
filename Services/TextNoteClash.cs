using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyTools.Services
{
    public class TextNoteClash
    {
        public static TextNote GetTextNoteInRegion(Face filledRegionFace, List<TextNote> allNotes)
        {
            Debug.WriteLine($"    [TextNoteClash] GetTextNoteInRegion START - checking {allNotes.Count} notes");

            if (filledRegionFace == null)
            {
                Debug.WriteLine("    [TextNoteClash] ERROR: filledRegionFace is null");
                return null;
            }

            foreach (TextNote note in allNotes)
            {
                XYZ notePoint = note.Coord;
                Debug.WriteLine($"    [TextNoteClash] Checking note '{note.Text.Substring(0, System.Math.Min(20, note.Text.Length))}...' at ({notePoint.X:F2}, {notePoint.Y:F2}, {notePoint.Z:F2})");

                if (FaceProjectionHelper.IsPointOnFace(filledRegionFace, notePoint))
                {
                    Debug.WriteLine($"    [TextNoteClash] FOUND: TextNote '{note.Text}' is inside region");
                    return note;
                }
            }

            Debug.WriteLine("    [TextNoteClash] GetTextNoteInRegion END - No matching note found");
            return null;
        }
    }
}