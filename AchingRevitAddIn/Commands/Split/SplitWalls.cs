#region namespaces
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Windows;
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
                //IList<Reference> refPlanePickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, refPlaneFilter, "Select reference planes");

                // Convert References to Elements
                IList<Element> walls = wallPickedReferences.Select(x => doc.GetElement(x)).ToList();
                //IList<Element> refPlanes = refPlanePickedReferences.Select(x => doc.GetElement(x)).ToList();

                using(Transaction trans = new Transaction(doc))
                {
                    trans.Start("Split walls");

                    foreach (Wall wall in walls)
                    {
                        Curve wallCurve = ((LocationCurve)wall.Location).Curve;
                        XYZ startPoint = wallCurve.GetEndPoint(0);
                        XYZ endPoint = wallCurve.GetEndPoint(1);

                        // Consider 2cm gap
                        double gap = 2.0;
                        double offset = gap / 2;

                        //offset = offset / (12 * 2.54);
                        UnitUtils.Convert(offset, DisplayUnitType.DUT_CENTIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);

                        // Consider splitting the wall in 4 parts
                        XYZ point1 = wallCurve.Evaluate(0.25, true);
                        XYZ point2 = wallCurve.Evaluate(0.5, true);
                        XYZ point3 = wallCurve.Evaluate(0.75, true);

                        // Create the 4 segments
                        Line line1 = Line.CreateBound(startPoint, point1);
                        Line line2 = Line.CreateBound(point1, point2);
                        Line line3 = Line.CreateBound(point2, point3);
                        Line line4 = Line.CreateBound(point3, endPoint);

                        double line1Start = line1.GetEndParameter(0);
                        double line1End = line1.GetEndParameter(1);
                        double line2Start = line2.GetEndParameter(0);
                        double line2End = line2.GetEndParameter(1);
                        double line3Start = line3.GetEndParameter(0);
                        double line3End = line3.GetEndParameter(1);
                        double line4Start = line4.GetEndParameter(0);
                        double line4End = line4.GetEndParameter(1);

                        line1.MakeBound(line1Start, line1End - offset);
                        line2.MakeBound(line2Start + offset, line2End - offset);
                        line3.MakeBound(line3Start + offset, line3End - offset);
                        line4.MakeBound(line4Start + offset, line4End);

                        // Diallow join at the end of the wall
                        if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                            WallUtils.DisallowWallJoinAtEnd(wall, 1);

                        // Create first wall
                        ((LocationCurve)wall.Location).Curve = line1;

                        // Create the next 3 walls
                        Wall wall2 = Wall.Create(doc, line2, wall.LevelId, true);
                        Wall wall3 = Wall.Create(doc, line3, wall.LevelId, true);
                        Wall wall4 = Wall.Create(doc, line4, wall.LevelId, true);

                        WallUtils.DisallowWallJoinAtEnd(wall2, 1);
                        WallUtils.DisallowWallJoinAtEnd(wall3, 1);
                        WallUtils.DisallowWallJoinAtEnd(wall4, 0);
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
