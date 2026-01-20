using Autodesk.Revit.DB;
using System.Collections.Generic;
using MyTools.Services;

namespace MyTools.Model
{
    public static class ViewSelected
    {
        
        private static ViewItem? _selectedView = null;         

        // Store selected view
        public static void SetSelectedView(ViewItem view)
        {
            _selectedView = view;
        }

        // Retrieve selected view
        public static ViewItem? GetSelectedView()
        {
            return _selectedView;
        }

        // Clear all selections
        public static void ClearSelections()
        {
            
            _selectedView = null;
        }

        // Check if selections exist
        public static bool HasSelection()
        {
            return _selectedView != null;
        }
    }
}