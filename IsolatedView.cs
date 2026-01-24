using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace MyTools
{
    /// <summary>
    /// Revit external application that creates the MyTools ribbon tab and button.
    /// </summary>
    public class IsolatedView : IExternalApplication
    {
        private const string TabName = "MyTools";
        private const string PanelName = "Selection Tools";
        private const string IconRelativePath = @"Resources\Icons\isolatelogo32.png";

        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbonTab(application);
            RibbonPanel panel = application.CreateRibbonPanel(TabName, PanelName);
            AddSelectCategoriesButton(panel);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateRibbonTab(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch
            {
                // Tab already exists
            }
        }

        private void AddSelectCategoriesButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                "SelectCategories",
                "Select\nElements",
                assemblyPath,
                "MyTools.SelectCategories"
            )
            {
                ToolTip = "Select elements in the active view and generate isolated 3D views"
            };

            SetButtonIcon(buttonData, assemblyPath);
            panel.AddItem(buttonData);
        }

        private void SetButtonIcon(PushButtonData buttonData, string assemblyPath)
        {
            string? assemblyFolder = Path.GetDirectoryName(assemblyPath);
            if (assemblyFolder == null)
                return;

            string iconPath = Path.Combine(assemblyFolder, IconRelativePath);

            if (!File.Exists(iconPath))
                return;

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                buttonData.LargeImage = bitmap;
            }
            catch
            {
                // Icon loading failed, button will use default icon
            }
        }
    }
}
