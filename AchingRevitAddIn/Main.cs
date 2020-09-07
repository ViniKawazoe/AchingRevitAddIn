#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Windows.Media.Imaging;
using AchingRevitAddIn.Resources;
using System.IO;
using System.Windows.Media;
#endregion

namespace AchingRevitAddIn
{
    public class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbon(application);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public void CreateRibbon(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            string tabName = "Aching";
            application.CreateRibbonTab(tabName);

            // Create a push button
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData strClmnNaming = new PushButtonData("StructuralColumnNamingButton", "Columns", path , "AchingRevitAddIn.StructuralColumnsNaming");
            PushButtonData strFrmnNaming = new PushButtonData("StructuralFramingNamingButton", "Beams", path, "AchingRevitAddIn.StructuralFramingNaming");
            PushButtonData floorNaming = new PushButtonData("FloorNamingButton", "Floors", path, "AchingRevitAddIn.FloorNaming");
            PushButtonData strFndtnNaming = new PushButtonData("StructuralFoundationNamingButton", "Foundations", path, "AchingRevitAddIn.StructuralFoundationNaming");
            
            // Create a ribbon panel
            RibbonPanel namingPanel = application.CreateRibbonPanel(tabName, "Element Naming");

            // Add buttons to the panel
            PushButton strColumnNamingButton = namingPanel.AddItem(strClmnNaming) as PushButton;
            PushButton strFrmnNamingButton = namingPanel.AddItem(strFrmnNaming) as PushButton;
            PushButton floorNamingButton = namingPanel.AddItem(floorNaming) as PushButton;
            PushButton strFndtnNamingButton = namingPanel.AddItem(strFndtnNaming) as PushButton;

            // Add button image
            strColumnNamingButton.LargeImage = PngImageSource("StrColumnNamingImage.png");
            strFrmnNamingButton.LargeImage = PngImageSource("StrFramingNamingImage.png");
            floorNamingButton.LargeImage = PngImageSource("FloorNamingImage.png");
            strFndtnNamingButton.LargeImage = PngImageSource("StrFoundationNamingImage.png");
        }

        private ImageSource PngImageSource (string ImageName)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AchingRevitAddIn.Images.Icons." + ImageName);
            PngBitmapDecoder image = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return image.Frames[0];
        }
    }
}
