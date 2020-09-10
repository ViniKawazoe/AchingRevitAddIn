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
#endregion

namespace AchingRevitAddIn
{
    [Transaction(TransactionMode.Manual)]
    class StructuralFramingNaming : IExternalCommand
    {
        private static UIDocument Uidoc { get; set; }

        #region public methods
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Send it to the public variable so other methods can call it
            Uidoc = uidoc;

            // Only allow view plans, 3D views, sections and elevations
            if (uidoc.ActiveView as ViewPlan == null &&
                uidoc.ActiveView as View3D == null &&
                uidoc.ActiveView as ViewSection == null)
            {
                return Result.Failed;
            }

            // Show UI

            return Result.Succeeded;
        }


        #endregion
    }
}
