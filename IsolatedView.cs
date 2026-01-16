using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;

namespace MyTools
{
    public class IsolatedView : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            string tabName = "MyTools";
            application.CreateRibbonTab(tabName);

            // Create a ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Selection Tools");

            // Get the assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Create button data for SelectElements command
            PushButtonData buttonData = new PushButtonData(
                "SelectCategories",           // Internal name
                "Select\nElements",         // Display name (use \n for two lines)
                assemblyPath,               // Path to DLL
                "MyTools.SelectCategories"    // Full class name of the command
            );

            // Add tooltip
            buttonData.ToolTip = "Select elements in the active view";

            // Add icon
            string assemblyFolder = Path.GetDirectoryName(assemblyPath);
            string iconPath = Path.Combine(assemblyFolder, "Resources", "Icons", "isolatelogo.png");
            if (File.Exists(iconPath))
            {
                buttonData.LargeImage = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }

            // Add the button to the ribbon panel
            PushButton pushButton = (PushButton)ribbonPanel.AddItem(buttonData) as PushButton;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
