using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;

namespace MyTools
{
    public class IsolatedView : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            Debug.WriteLine("=== IsolatedView.OnStartup START ===");

            // Create a custom ribbon tab
            string tabName = "MyTools";
            try
            {
                application.CreateRibbonTab(tabName);
                Debug.WriteLine($"Created ribbon tab: {tabName}");
            }
            catch
            {
                Debug.WriteLine($"Ribbon tab '{tabName}' already exists");
            }

            // 2. Create the panel on that tab
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Selection Tools");
            Debug.WriteLine("Created ribbon panel: Selection Tools");

            // Get the assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            Debug.WriteLine($"Assembly path: {assemblyPath}");

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
            string iconPath = Path.Combine(assemblyFolder, "Resources", "Icons", "isolatelogo32.png");
            Debug.WriteLine($"Icon path: {iconPath}");
            Debug.WriteLine($"Icon exists: {File.Exists(iconPath)}");

            if (File.Exists(iconPath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    buttonData.LargeImage = bitmap;
                    Debug.WriteLine("Icon loaded successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR loading icon: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("WARNING: Icon file not found!");
            }

            // Add the button to the ribbon panel
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            Debug.WriteLine("Button added to ribbon panel");

            Debug.WriteLine("=== IsolatedView.OnStartup END ===");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
