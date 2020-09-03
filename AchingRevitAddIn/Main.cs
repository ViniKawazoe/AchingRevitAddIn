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
            PushButtonData strClmnNaming = new PushButtonData("StructuralColumnNamingButton", "Column Naming", path , "AchingRevitAddIn.StructuralColumnsNaming");
            
            // Create a ribbon panel
            RibbonPanel namingPanel = application.CreateRibbonPanel(tabName, "Element Naming");

            // Add button image
            BitmapImage strColumnNamingImage = new BitmapImage(new Uri(@"C:\Users\Avell\Google Drive\P1 - 32pix - v8.png"));
            

            // Add buttons to the panel
            PushButton strColumnNamingButton = namingPanel.AddItem(strClmnNaming) as PushButton;
            strColumnNamingButton.LargeImage = strColumnNamingImage;
            //strColumnNamingButton.LargeImage = ResourceImage.GetIcon("P1 - 32pix - v1.png");
        }
    }
}
