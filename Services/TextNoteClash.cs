using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyTools.Services
{
    public class TextNoteClash
    {
        public static TextNote GetTextNoteInRegion(IList<CurveLoop> regionBoundaries, List<TextNote> allNotes)
        {
            Debug.WriteLine($"    [TextNoteClash] GetTextNoteInRegion START - checking {allNotes.Count} notes");

            if (regionBoundaries == null || regionBoundaries.Count == 0)
            {
                Debug.WriteLine("    [TextNoteClash] ERROR: regionBoundaries is null or empty");
                return null;
            }

            int checkedCount = 0;
            foreach (TextNote note in allNotes)
            {
                checkedCount++;
                XYZ notePoint = note.Coord;
                string shortText = note.Text.Length > 30 ? note.Text.Substring(0, 30) + "..." : note.Text;

                Debug.WriteLine($"    [TextNoteClash] Checking note {checkedCount}/{allNotes.Count}: '{shortText}' at ({notePoint.X:F2}, {notePoint.Y:F2})");

                if (FaceProjectionHelper.IsPointInsideBoundary(regionBoundaries, notePoint))
                {
                    Debug.WriteLine($"    [TextNoteClash] FOUND: TextNote '{shortText}' is inside region");
                    return note;
                }
            }

            Debug.WriteLine($"    [TextNoteClash] GetTextNoteInRegion END - No matching note found");
            return null;
        }
    }
}
