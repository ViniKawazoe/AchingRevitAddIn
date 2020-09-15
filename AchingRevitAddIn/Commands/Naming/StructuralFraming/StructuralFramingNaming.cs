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
using AchingRevitAddIn.Filters;
using Autodesk.Revit.ApplicationServices;
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
            StructuralFramingNamingUI structuralFramingNamingWindow = new StructuralFramingNamingUI();
            structuralFramingNamingWindow.ShowDialog();

            return Result.Succeeded;
        }

        static internal void NameFramings(string prefix, int initialNumber, int sortVertical, int sortHorizontal)
        {
            try
            {
                // Get UIDocument and Document
                UIDocument uidoc = Uidoc;
                Document doc = uidoc.Document;
                Application app = doc.Application;

                // Create filter
                StructuralFramingFilter filter = new StructuralFramingFilter();

                // Apply filter and select multiple structural framings
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select Structural Framings");

                // Convert References to Elements
                IList<Element> strFramings = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                IList<Element> horizontalFramings = new List<Element>();
                IList<Element> verticalFramings = new List<Element>();

                // Separate horizontal and vertical beams 
                foreach (Element beam in strFramings)
                {
                    IList<Curve> curves = new List<Curve>();
                    XYZ startPoint = ((LocationCurve)beam.Location).Curve.GetEndPoint(0);
                    XYZ endPoint = ((LocationCurve)beam.Location).Curve.GetEndPoint(1);

                    XYZ vector = (endPoint - startPoint).Normalize();
                    double vX = vector.X;
                    double vY = vector.Y;

                    if (vX < 0) { vX *= -1; }
                    if (vY < 0) { vY *= -1; }

                    if (vX >= vY)
                    {
                        horizontalFramings.Add(beam);
                    }
                    else
                    {
                        verticalFramings.Add(beam);
                    }
                }

                // Sort by center point
                IList<Element> sortedHorizontalFramings = new List<Element>();
                IList<Element> sortedVerticalFramings = new List<Element>();

                if (horizontalFramings != null)
                {
                    sortedHorizontalFramings = SortHorizontalFramings(horizontalFramings, sortHorizontal, sortVertical);
                }
                if (verticalFramings != null)
                {
                    sortedVerticalFramings = SortVerticalFramings(verticalFramings, sortHorizontal, sortVertical);
                }

                // Join both lists
                IList<Element> sortedFramings = new List<Element>();

                if (sortedHorizontalFramings != null)
                {
                    foreach (Element framing in sortedHorizontalFramings)
                    {
                        sortedFramings.Add(framing);
                    }
                }
                if (sortedVerticalFramings != null)
                {
                    foreach (Element framing in sortedVerticalFramings)
                    {
                        sortedFramings.Add(framing);
                    }
                }

                foreach (Element elem in sortedFramings)
                {
                    RotateFraming(elem);
                }

                string s = "";
                int count = 1;
                foreach (Element fm in sortedFramings)
                {
                    XYZ startPoint = ((LocationCurve)fm.Location).Curve.GetEndPoint(0);
                    XYZ middlePoint = ((LocationCurve)fm.Location).Curve.Evaluate(0.5, true);
                    XYZ endPoint = ((LocationCurve)fm.Location).Curve.GetEndPoint(1);
                    XYZ vector = (endPoint - startPoint).Normalize();

                    s = s + "Framing " + count
                        + "\n"
                        + "Start point: " + startPoint.ToString()
                        + "\n"
                        + "Center point: " + middlePoint.ToString()
                        + "\n"
                        + "End point: " + endPoint.ToString()
                        + "\n"
                        + "Vector: " + vector.ToString()
                        + "\n"
                        + "-------------------------------------------------------"
                        + "\n";

                    count++;
                }

                TaskDialog.Show("Framings", s);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sort horizontal framings
        /// </summary>
        /// <param name="horizontalFramings"></param>
        /// <param name="sortHorizontal"></param>
        /// <param name="sortVertical"></param>
        /// <returns></returns>
        static internal List<Element> SortHorizontalFramings(IList<Element> horizontalFramings, int sortHorizontal, int sortVertical)
        {
            List<Element> sortedHorizontalFramings = null;

            if (sortVertical == 0 && sortHorizontal == 0)
            {
                sortedHorizontalFramings = horizontalFramings.
                    OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ToList();
            }
            if (sortVertical == 0 && sortHorizontal == 1)
            {
                sortedHorizontalFramings = horizontalFramings.
                    OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 0)
            {
                sortedHorizontalFramings = horizontalFramings.
                    OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 1)
            {
                sortedHorizontalFramings = horizontalFramings.
                    OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ToList();
            }

            return sortedHorizontalFramings;
        }

        /// <summary>
        /// Sort vertical framings
        /// </summary>
        /// <param name="verticalFramings"></param>
        /// <param name="sortHorizontal"></param>
        /// <param name="sortVertical"></param>
        /// <returns></returns>
        static internal List<Element> SortVerticalFramings(IList<Element> verticalFramings, int sortHorizontal, int sortVertical)
        {
            List<Element> sortedVerticalFramings = null;

            if (sortVertical == 0 && sortHorizontal == 0)
            {
                sortedVerticalFramings = verticalFramings.
                    OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ToList();
            }
            if (sortVertical == 0 && sortHorizontal == 1)
            {
                sortedVerticalFramings = verticalFramings.
                    OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 0)
            {
                sortedVerticalFramings = verticalFramings.
                    OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 1)
            {
                sortedVerticalFramings = verticalFramings.
                    OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                    ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                    ToList();
            }

            return sortedVerticalFramings;
        }

        static internal void RotateFraming(Element framing)
        {
            using (Transaction trans = new Transaction(Uidoc.Document))
            {
                trans.Start("Rotate element");

                LocationCurve curve = framing.Location as LocationCurve;
                XYZ startPoint = curve.Curve.GetEndPoint(0);
                XYZ middlePoint = curve.Curve.Evaluate(0.5, true);
                XYZ endPoint = curve.Curve.GetEndPoint(1);
                XYZ middleHigh = middlePoint.Add(XYZ.BasisZ);
                Line axisLine = Line.CreateBound(middlePoint, middleHigh);

                XYZ vector = (endPoint - startPoint).Normalize();

                double vectorX = vector.X;
                double vectorY = vector.Y;

                if ((vectorX == 0 && vectorY < 0) || (vectorX < 0 && vectorY == 0))
                {
                    //curve.Rotate(axisLine, Math.PI);
                    ((LocationCurve)framing.Location).Rotate(axisLine, Math.PI);
                }

                trans.Commit();
            }
        }

        #endregion
    }
}
