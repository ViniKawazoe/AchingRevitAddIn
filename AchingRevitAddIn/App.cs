using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AchingRevitAddIn
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            AddTab(application);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        private void AddTab(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            string tabName = "Aching";
            application.CreateRibbonTab(tabName);

            // Create a push button
            PushButtonData button1 = new PushButtonData("Button 1", "Nomear pilares", @"C:\AchingRevitAddIn\AchingRevitAddIn\bin\Debug\AchingRevitAddIn.dll", "AchingRevitAddIn.Functions.Naming.StructuralColumnNaming");

            // Create a ribbon panel
            RibbonPanel namingPanel = application.CreateRibbonPanel(tabName, "Nomear elementos");

            // Add buttons to the panel
            namingPanel.AddItem(button1);

        }
    }
}
