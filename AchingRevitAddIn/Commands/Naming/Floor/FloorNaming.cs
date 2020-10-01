#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using AchingRevitAddIn.Filters;
#endregion

namespace AchingRevitAddIn
{
    [Transaction(TransactionMode.Manual)]
    class FloorNaming : IExternalCommand
    {
        private static UIDocument Uidoc { get; set; }

        #region public methods
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Send to the public variable so other methods can call it
            Uidoc = uidoc;

            // Only allow view plans, 3D views, sections and elevations
            if (uidoc.ActiveView as ViewPlan == null &&
                uidoc.ActiveView as View3D == null &&
                uidoc.ActiveView as ViewSection == null)
            {
                return Result.Failed;
            }

            // Show UI
            FloorNamingUI floorNamingWindow = new FloorNamingUI();
            floorNamingWindow.ShowDialog();

            return Result.Succeeded;
        }

        static internal void NameFloors(string prefix, int initialNumber, int sortVertical, int sortHorizontal)
        {
            try
            {
                // Get UIDocument and Document
                UIDocument uidoc = Uidoc;
                Document doc = uidoc.Document;
                Application app = doc.Application;

                // Create filter
                FloorFilter filter = new FloorFilter();

                // Apply filter and select multiple floors
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select floors");

                // Convert References to Elements
                IList<Element> floors = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                IList<Element> sortedFloors = null;

                // Sort the floors by location
                if (sortVertical == 0 && sortHorizontal == 0)
                {
                    sortedFloors = floors;
                }
                if (sortVertical == 0 && sortHorizontal == 1)
                {

                }
                if (sortVertical == 1 && sortHorizontal == 0)
                {

                }
                if (sortVertical == 1 && sortHorizontal == 1)
                {

                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
