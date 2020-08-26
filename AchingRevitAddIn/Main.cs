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

namespace AchingRevitAddIn
{
    public class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            string tabName = "Aching";
            application.CreateRibbonTab(tabName);

            // Create a push button
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData strClmnNaming = new PushButtonData("Button 1", "Nomear pilares", path, "AchingRevitAddIn.StructuralColumnNaming");

            // Create a ribbon panel
            RibbonPanel namingPanel = application.CreateRibbonPanel(tabName, "Nomear elementos");

            // Add button image
            // Create later

            // Add buttons to the panel
            namingPanel.AddItem(strClmnNaming);
            

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
