using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using MyTools.Services;

namespace MyTools.Model
{
    public class TextNoteSelect
    {
        /// <summary>
        /// Retrieves all TextNote elements visible in the provided 2D View.
        /// </summary>
        public static List<TextNote> GetTextNotesInView(View currentView)
        {
            Document doc = currentView.Document;

            // Use the collector constructor that takes a ViewId to limit results to that view
            return new FilteredElementCollector(doc, currentView.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .ToList();
        }
    }
}