#region namespaces
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
#endregion

namespace AchingRevitAddIn
{
    [Transaction(TransactionMode.Manual)]
    class SplitWalls : IExternalCommand
    {
        #region public methods
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Only allow view plans, 3D views, sections and elevations
            if (uidoc.ActiveView as ViewPlan == null &&
                uidoc.ActiveView as View3D == null &&
                uidoc.ActiveView as ViewSection == null)
            {
                return Result.Failed;
            }

            try
            {
                // Create filters
                WallFilter wallFilter = new WallFilter();
                ReferencePlaneFilter refPlaneFilter = new ReferencePlaneFilter();

                // Apply filter and select multiple walls
                IList<Reference> wallPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, wallFilter, "Select walls");

                // Apply filter and select multiple reference planes
                IList<Reference> refPlanePickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, refPlaneFilter, "Select reference planes");

                // Convert References to Elements
                IList<Element> walls = wallPickedReferences.Select(x => doc.GetElement(x)).ToList();
                IList<Element> refPlanes = refPlanePickedReferences.Select(x => doc.GetElement(x)).ToList();

                using(Transaction trans = new Transaction(doc))
                {
                    trans.Start("Split walls");

                    foreach (Wall wall in walls)
                    {
                        Curve wallCurve = ((LocationCurve)wall.Location).Curve;
                        XYZ startPoint = wallCurve.GetEndPoint(0);
                        XYZ endPoint = wallCurve.GetEndPoint(1);


                    }

                    trans.Commit();
                }

            }
            catch
            {
            }

            return Result.Succeeded;
        }

        #endregion
    }
}
