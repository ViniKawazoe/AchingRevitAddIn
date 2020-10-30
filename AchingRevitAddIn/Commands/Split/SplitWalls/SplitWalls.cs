#region namespaces
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System;
#endregion

namespace AchingRevitAddIn
{
    [Transaction(TransactionMode.Manual)]
    class SplitWalls : IExternalCommand
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

            SplitWallsUI splitWallsUI = new SplitWallsUI();
            splitWallsUI.ShowDialog();

            return Result.Succeeded;
        }

        /// <summary>
        /// Main method to call the other methods
        /// </summary>
        /// <param name="gap"></param>
        /// <param name="divisions"></param>
        /// <param name="divisionType"></param>
        static internal void SplitWall(double gap, int divisions, int divisionType)
        {
            try
            {
                UIDocument uidoc = Uidoc;
                Document doc = uidoc.Document;

                // Create filters
                WallFilter wallFilter = new WallFilter();
                GridFilter gridFilter = new GridFilter();

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Split walls");

                    if (divisionType == 0)
                    {
                        // Apply filter and select multiple walls
                        IList<Reference> wallPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, wallFilter, "Select walls");

                        // Apply filter and select multiple grids
                        IList<Reference> gridPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, gridFilter, "Select grids");

                        // Convert References to Elements
                        IList<Element> walls = wallPickedReferences.Select(x => doc.GetElement(x)).ToList();
                        IList<Element> grids = gridPickedReferences.Select(x => doc.GetElement(x)).ToList();

                        SplitWalls_Grids(walls, grids, gap);

                    }
                    else if (divisionType == 1)
                    {
                        // Apply filter and select multiple walls
                        IList<Reference> wallPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, wallFilter, "Select walls");

                        // Convert References to Elements
                        IList<Element> walls = wallPickedReferences.Select(x => doc.GetElement(x)).ToList();

                        //SplitWalls_EqualDivisions(walls, gap, divisions);
                        IList<Element> splittedWalls = SplitWalls_EqualDivisions(walls, gap, divisions);

                        doc.Regenerate();

                        for (int i = 0; i < splittedWalls.Count; i++)
                        {
                            AddVerticalReveals(splittedWalls[i] as Wall);
                        }

                    }
                    else
                    {
                        // Apply filter and select multiple walls
                        IList<Reference> wallPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, wallFilter, "Select walls");

                        // Apply filter and select multiple grids
                        IList<Reference> gridPickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, gridFilter, "Select grids");

                        // Convert References to Elements
                        IList<Element> walls = wallPickedReferences.Select(x => doc.GetElement(x)).ToList();
                        IList<Element> grids = gridPickedReferences.Select(x => doc.GetElement(x)).ToList();

                        IList<Element> splittedWalls = SplitWalls_Grids(walls, grids, gap);
                        SplitWalls_EqualDivisions(splittedWalls, gap, divisions);
                    }

                    trans.Commit();
                }

            }
            catch
            {
            }
        }

        /// <summary>
        /// Get the intersection point between two lines
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        static internal XYZ GetIntersections(Line line1, Line line2)
        {
            IntersectionResultArray results;

            SetComparisonResult result = line1.Intersect(line2, out results);

            if (result != SetComparisonResult.Overlap)
            {
                throw new System.InvalidOperationException("Input lines do not intersect");
            }
            if (results == null || results.Size != 1)
            {
                throw new System.InvalidOperationException("Could not extract line intersection point");
            }

            IntersectionResult iResult = results.get_Item(0);

            return iResult.XYZPoint;
        }

        /// <summary>
        /// Sort the points according to the curve's direction
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="points"></param>
        static internal IList<XYZ> OrganizePointsAlongLine(Line curve, IList<XYZ> points)
        {
            XYZ curveStartPoint = curve.GetEndPoint(0);
            XYZ curveEndPoint = curve.GetEndPoint(1);

            double startPointX = curveStartPoint.X;
            double startPointY = curveStartPoint.Y;
            double endPointX = curveEndPoint.X;
            double endPointY = curveEndPoint.Y;

            double deltaX = Math.Round(endPointX - startPointX, 5);
            double deltaY = Math.Round(endPointY - startPointY, 5);

            if (deltaX == 0 || deltaY == 0)
            {
                // The line is in the ordinate axis
                if (deltaX == 0)
                {
                    // Upwards
                    if (deltaY > 0)
                    {
                        points = points.OrderBy(point => point.Y)
                            .ToList();
                    }
                    // Downwards
                    else
                    {
                        points = points.OrderByDescending(point => point.Y)
                            .ToList();
                    }
                }
                // The line is in the abscissa axis
                else
                {
                    // Left to Right
                    if (deltaX > 0)
                    {
                        points = points.OrderBy(point => point.X)
                            .ToList();
                    }
                    // Right to Left
                    else
                    {
                        points = points.OrderByDescending(point => point.X)
                            .ToList();
                    }
                }
            }
            else
            {
                // Left -> Right | Upwards
                if (deltaX > 0 && deltaY > 0)
                {
                    points = points.OrderBy(point => point.X)
                        .ThenBy(point => point.Y)
                        .ToList();
                }
                // Right -> Left | Upwards
                else if (deltaX < 0 && deltaY > 0)
                {
                    points = points.OrderByDescending(point => point.X)
                        .ThenBy(point => point.Y)
                        .ToList();
                }
                // Right -> Left | Downwards
                else if (deltaX < 0 && deltaY < 0)
                {
                    points = points.OrderByDescending(point => point.X)
                        .ThenByDescending(point => point.Y)
                        .ToList();
                }
                // Left -> Right | Downwards
                else if (deltaX > 0 && deltaY < 0)
                {
                    points = points.OrderBy(point => point.X)
                        .ThenByDescending(point => point.Y)
                        .ToList();
                }
            }

            return points;
        }

        /// <summary>
        /// Split the walls in the intersection between the selected grids
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="grids"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        static internal List<Element> SplitWalls_Grids(IList<Element> walls, IList<Element> grids, double gap)
        {
            UIDocument uidoc = Uidoc;
            Document doc = uidoc.Document;

            IList<Element> splittedWalls = new List<Element>();

            foreach (Wall wall in walls)
            {
                Curve wallCurve = ((LocationCurve)wall.Location).Curve;
                XYZ startPoint = wallCurve.GetEndPoint(0);
                XYZ endPoint = wallCurve.GetEndPoint(1);
                XYZ middlePoint = wallCurve.Evaluate(0.5, true);

                double wallZ = middlePoint.Z;

                ElementId wallTypeId = wall.WallType.Id;
                ElementId wallLevelId = wall.LevelId;
                ElementId topConstrain = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
                double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                double baseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                double topOffset = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();

                // get offset
                double offset = 0.0;
                if (gap != 0)
                {
                    offset = gap / 2;
                    offset /= (12 * 2.54); // feet to centimeter
                }

                IList<XYZ> intersectionPoints = new List<XYZ>();
                foreach (Grid grid in grids)
                {
                    Curve gridCurve = grid.Curve;
                    XYZ gridStartPoint = gridCurve.GetEndPoint(0);
                    XYZ gridEndPoint = gridCurve.GetEndPoint(1);

                    double startPointX = gridStartPoint.X;
                    double startPointY = gridStartPoint.Y;
                    double endPointX = gridEndPoint.X;
                    double endPointY = gridEndPoint.Y;

                    XYZ lineStartPoint = new XYZ(startPointX, startPointY, wallZ);
                    XYZ lineEndPoint = new XYZ(endPointX, endPointY, wallZ);

                    Line gridLine = Line.CreateBound(lineStartPoint, lineEndPoint);

                    XYZ intersectionPoint;

                    try
                    {
                        intersectionPoint = GetIntersections(wallCurve as Line, gridLine);
                    }
                    catch
                    {
                        intersectionPoint = null;
                    }

                    if (intersectionPoint != null)
                    {
                        intersectionPoints.Add(intersectionPoint);
                    }
                }

                // Sort the points according the curve's direction
                intersectionPoints = OrganizePointsAlongLine(wallCurve as Line, intersectionPoints);

                if (intersectionPoints != null)
                {
                    if (intersectionPoints.Count == 1)
                    {
                        Line line1 = Line.CreateBound(startPoint, intersectionPoints[0]);
                        Line line2 = Line.CreateBound(intersectionPoints[0], endPoint);
                        double line1Start = line1.GetEndParameter(0);
                        double line1End = line1.GetEndParameter(1);
                        double line2Start = line2.GetEndParameter(0);
                        double line2End = line2.GetEndParameter(1);

                        line1.MakeBound(line1Start, line1End - offset);
                        line2.MakeBound(line2Start + offset, line2End);

                        Wall wall2 = Wall.Create(doc, line2, wallTypeId, wallLevelId, wallHeight, baseOffset, false, true);
                        Parameter topCons = wall2.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                        if (topCons != null)
                        {
                            topCons.Set(topConstrain);
                        }
                        Parameter topOff = wall2.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                        topOff.Set(topOffset);

                        ((LocationCurve)wall.Location).Curve = line1;
                        ((LocationCurve)wall2.Location).Curve = line2;

                        if (WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
                            WallUtils.DisallowWallJoinAtEnd(wall, 0);
                        if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                            WallUtils.DisallowWallJoinAtEnd(wall, 1);
                        if (WallUtils.IsWallJoinAllowedAtEnd(wall2, 0))
                            WallUtils.DisallowWallJoinAtEnd(wall2, 0);
                        if (WallUtils.IsWallJoinAllowedAtEnd(wall2, 1))
                            WallUtils.DisallowWallJoinAtEnd(wall2, 1);

                        splittedWalls.Add(wall);
                        splittedWalls.Add(wall2);
                    }
                    else
                    {
                        IList<Line> lines = new List<Line>();

                        for (int i = 0; i < intersectionPoints.Count; i++)
                        {
                            // First grid
                            if (i == 0)
                            {
                                // Create first line
                                Line line = Line.CreateBound(startPoint, intersectionPoints[i]);
                                double lineStart = line.GetEndParameter(0);
                                double lineEnd = line.GetEndParameter(1);

                                line.MakeBound(lineStart, lineEnd - offset);

                                // Create second line
                                Line line2 = Line.CreateBound(intersectionPoints[i], intersectionPoints[i + 1]);
                                double line2Start = line2.GetEndParameter(0);
                                double line2End = line2.GetEndParameter(1);

                                line2.MakeBound(line2Start + offset, line2End - offset);

                                lines.Add(line);
                                lines.Add(line2);
                            }
                            // Last grid
                            else if (i == intersectionPoints.Count - 1)
                            {
                                Line line = Line.CreateBound(intersectionPoints[i], endPoint);
                                double lineStart = line.GetEndParameter(0);
                                double lineEnd = line.GetEndParameter(1);

                                line.MakeBound(lineStart + offset, lineEnd);

                                lines.Add(line);
                            }
                            // Intermediate grids
                            else
                            {
                                Line line = Line.CreateBound(intersectionPoints[i], intersectionPoints[i + 1]);
                                double lineStart = line.GetEndParameter(0);
                                double lineEnd = line.GetEndParameter(1);

                                line.MakeBound(lineStart + offset, lineEnd - offset);

                                lines.Add(line);
                            }
                        }

                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (i == 0)
                            {
                                ((LocationCurve)wall.Location).Curve = lines[i];

                                if (WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
                                    WallUtils.DisallowWallJoinAtEnd(wall, 0);
                                if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                                    WallUtils.DisallowWallJoinAtEnd(wall, 1);

                                splittedWalls.Add(wall);
                            }
                            else
                            {
                                Wall newWall = Wall.Create(doc, lines[i], wallTypeId, wallLevelId, wallHeight, baseOffset, false, true);

                                Parameter topCons = newWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                                if (topCons != null)
                                {
                                    topCons.Set(topConstrain);
                                }
                                Parameter topOff = newWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                                topOff.Set(topOffset);

                                if (WallUtils.IsWallJoinAllowedAtEnd(newWall, 1))
                                    WallUtils.DisallowWallJoinAtEnd(newWall, 1);
                                if (WallUtils.IsWallJoinAllowedAtEnd(newWall, 0))
                                    WallUtils.DisallowWallJoinAtEnd(newWall, 0);

                                splittedWalls.Add(newWall);
                            }
                        }
                    }
                }
            }

            return splittedWalls as List<Element>;
        }

        /// <summary>
        /// Split walls into equal elements
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="gap"></param>
        /// <param name="divisions"></param>
        /// <returns></returns>
        static internal List<Element> SplitWalls_EqualDivisions(IList<Element> walls, double gap, int divisions)
        {
            UIDocument uidoc = Uidoc;
            Document doc = uidoc.Document;

            IList<Element> splittedWalls = new List<Element>();

            foreach (Wall wall in walls)
            {
                Curve wallCurve = ((LocationCurve)wall.Location).Curve;
                XYZ startPoint = wallCurve.GetEndPoint(0);
                XYZ endPoint = wallCurve.GetEndPoint(1);
                XYZ middlePoint = wallCurve.Evaluate(0.5, true);

                ElementId wallTypeId = wall.WallType.Id;
                ElementId wallLevelId = wall.LevelId;
                ElementId topConstrain = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
                double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                double baseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                double topOffset = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();

                // get offset
                double offset = 0.0;
                if (gap != 0)
                {
                    offset = gap / 2;
                    offset = offset / (12 * 2.54); // feet to centimeter
                }

                if (divisions == 1)
                {
                    splittedWalls.Add(wall);
                }
                else if (divisions == 2)
                {
                    Line line1 = Line.CreateBound(startPoint, middlePoint);
                    Line line2 = Line.CreateBound(middlePoint, endPoint);
                    double line1Start = line1.GetEndParameter(0);
                    double line1End = line1.GetEndParameter(1);
                    double line2Start = line2.GetEndParameter(0);
                    double line2End = line2.GetEndParameter(1);

                    line1.MakeBound(line1Start, line1End - offset);
                    line2.MakeBound(line2Start + offset, line2End);

                    Wall wall2 = Wall.Create(doc, line2, wallTypeId, wallLevelId, wallHeight, baseOffset, false, true);
                    Parameter topCons = wall2.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                    if (topCons != null)
                    {
                        topCons.Set(topConstrain);
                    }
                    Parameter topOff = wall2.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                    topOff.Set(topOffset);

                    ((LocationCurve)wall.Location).Curve = line1;
                    ((LocationCurve)wall2.Location).Curve = line2;

                    if (WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
                        WallUtils.DisallowWallJoinAtEnd(wall, 0);
                    if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                        WallUtils.DisallowWallJoinAtEnd(wall, 1);
                    if (WallUtils.IsWallJoinAllowedAtEnd(wall2, 0))
                        WallUtils.DisallowWallJoinAtEnd(wall2, 0);
                    if (WallUtils.IsWallJoinAllowedAtEnd(wall2, 1))
                        WallUtils.DisallowWallJoinAtEnd(wall2, 1);

                    splittedWalls.Add(wall);
                    splittedWalls.Add(wall2);
                }
                else
                {
                    IList<Line> lines = new List<Line>();

                    for (int i = 1; i < divisions; i++)
                    {
                        double segmentLenghtNormalized = 1.0 / (double)divisions;
                        double segment = segmentLenghtNormalized * i;
                        XYZ currentPoint = wallCurve.Evaluate(segment, true);

                        // First line
                        if (i == 1)
                        {
                            // Create first line
                            Line line = Line.CreateBound(startPoint, currentPoint);
                            double lineStart = line.GetEndParameter(0);
                            double lineEnd = line.GetEndParameter(1);

                            line.MakeBound(lineStart, lineEnd - offset);

                            // Create second line
                            XYZ nextPoint = wallCurve.Evaluate(segment + segmentLenghtNormalized, true);
                            Line line2 = Line.CreateBound(currentPoint, nextPoint);
                            double line2Start = line2.GetEndParameter(0);
                            double line2End = line2.GetEndParameter(1);

                            line2.MakeBound(line2Start + offset, line2End - offset);

                            lines.Add(line);
                            lines.Add(line2);
                        }
                        // Last line
                        else if (i == divisions - 1)
                        {
                            Line line = Line.CreateBound(currentPoint, endPoint);
                            double lineStart = line.GetEndParameter(0);
                            double lineEnd = line.GetEndParameter(1);

                            line.MakeBound(lineStart + offset, lineEnd);

                            lines.Add(line);
                        }
                        // Intermediate lines
                        else
                        {
                            XYZ nextPoint = wallCurve.Evaluate(segment + segmentLenghtNormalized, true);
                            Line line = Line.CreateBound(currentPoint, nextPoint);
                            double lineStart = line.GetEndParameter(0);
                            double lineEnd = line.GetEndParameter(1);

                            line.MakeBound(lineStart + offset, lineEnd - offset);

                            lines.Add(line);
                        }
                    }

                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((LocationCurve)wall.Location).Curve = lines[i];

                            if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                                WallUtils.DisallowWallJoinAtEnd(wall, 1);

                            splittedWalls.Add(wall);
                        }
                        else
                        {
                            Wall newWall = Wall.Create(doc, lines[i], wallTypeId, wallLevelId, wallHeight, baseOffset, false, true);

                            Parameter topCons = newWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                            if (topCons != null)
                            {
                                topCons.Set(topConstrain);
                            }
                            Parameter topOff = newWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                            topOff.Set(topOffset);

                            if (WallUtils.IsWallJoinAllowedAtEnd(newWall, 0))
                                WallUtils.DisallowWallJoinAtEnd(newWall, 0);
                            if (WallUtils.IsWallJoinAllowedAtEnd(newWall, 1))
                                WallUtils.DisallowWallJoinAtEnd(newWall, 1);

                            splittedWalls.Add(newWall);
                        }
                    }
                }
            }

            return splittedWalls as List<Element>;
        }

        /// <summary>
        /// Insert vertical reveals on the four corners of a wall
        /// </summary>
        /// <param name="wall"></param>
        static internal void AddVerticalReveals(Wall wall)
        {
            UIDocument uidoc = Uidoc;
            Document doc = uidoc.Document;

            Curve wallCurve = ((LocationCurve)wall.Location).Curve;
            double curveStart = wallCurve.GetEndParameter(0);
            double curveEnd = wallCurve.GetEndParameter(1);

            // Get the reveal profile
            ElementType wallRevealType = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Reveals)
                .WhereElementIsElementType()
                .Cast<ElementType>()
                .FirstOrDefault();

            if (wallRevealType != null)
            {
                var wallRevealInfo1 = new WallSweepInfo(WallSweepType.Reveal, true);
                wallRevealInfo1.DistanceMeasuredFrom = DistanceMeasuredFrom.Base;
                wallRevealInfo1.Distance = curveStart;
                wallRevealInfo1.WallSide = WallSide.Exterior;
                wallRevealInfo1.IsProfileFlipped = false;

                var wallRevealInfo2 = new WallSweepInfo(WallSweepType.Reveal, true);
                wallRevealInfo2.DistanceMeasuredFrom = DistanceMeasuredFrom.Base;
                wallRevealInfo2.Distance = curveStart;
                wallRevealInfo2.WallSide = WallSide.Interior;
                wallRevealInfo2.IsProfileFlipped = true;

                var wallRevealInfo3 = new WallSweepInfo(WallSweepType.Reveal, true);
                wallRevealInfo3.DistanceMeasuredFrom = DistanceMeasuredFrom.Top;
                wallRevealInfo3.Distance = curveEnd;
                wallRevealInfo3.WallSide = WallSide.Exterior;
                wallRevealInfo3.IsProfileFlipped = true;

                var wallRevealInfo4 = new WallSweepInfo(WallSweepType.Reveal, true);
                wallRevealInfo4.DistanceMeasuredFrom = DistanceMeasuredFrom.Top;
                wallRevealInfo4.Distance = curveEnd;
                wallRevealInfo4.WallSide = WallSide.Interior;
                wallRevealInfo4.IsProfileFlipped = false;

                WallSweep.Create(wall, wallRevealType.Id, wallRevealInfo1);
                WallSweep.Create(wall, wallRevealType.Id, wallRevealInfo2);
                WallSweep.Create(wall, wallRevealType.Id, wallRevealInfo3);
                WallSweep.Create(wall, wallRevealType.Id, wallRevealInfo4);
            }
            else
            {
                TaskDialog.Show("ERROR", "No wall reveal type was found!");
            }
        }

        #endregion
    }
}
