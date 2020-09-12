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
                SortFramings(horizontalFramings, verticalFramings, sortHorizontal, sortVertical);

                // Join both lists
                IList<Element> sortedFramings = new List<Element>();

                if (horizontalFramings != null)
                {
                    foreach (Element framing in horizontalFramings)
                    {
                        sortedFramings.Add(framing);
                    }
                }
                if (verticalFramings != null)
                {
                    foreach (Element framing in verticalFramings)
                    {
                        sortedFramings.Add(framing);
                    }
                }

                string s = "";
                int count = 1;
                /*
                foreach (Element fm in sortedFramings)
                {
                    s = s + "Framing " + count
                        + "\n"
                        + "Start point: " + ((LocationCurve)fm.Location).Curve.GetEndPoint(0).ToString()
                        + "\n"
                        + "Center point: " + ((LocationCurve)fm.Location).Curve.Evaluate(0.5, true).ToString()
                        + "\n"
                        + "SEnd point: " + ((LocationCurve)fm.Location).Curve.GetEndPoint(1).ToString()
                        + "\n"
                        + "-------------------------------------------------------"
                        + "\n";

                    count++;
                }
                */
                for (int i = 0; i < sortedFramings.Count; i++)
                {
                    s = s + "Framing " + count
                        + "\n"
                        + "Start point: " + ((LocationCurve)sortedFramings[i].Location).Curve.GetEndPoint(0).ToString()
                        + "\n"
                        + "Center point: " + ((LocationCurve)sortedFramings[i].Location).Curve.Evaluate(0.5, true).ToString()
                        + "\n"
                        + "SEnd point: " + ((LocationCurve)sortedFramings[i].Location).Curve.GetEndPoint(1).ToString()
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
        ///  Sort framings
        /// </summary>
        /// <param name="horizontalFramings"></param>
        /// <param name="verticalFramings"></param>
        /// <param name="sortHorizontal"></param>
        /// <param name="sortVertical"></param>
        static internal void SortFramings(IList<Element> horizontalFramings, IList<Element> verticalFramings, int sortHorizontal, int sortVertical)
        {
            if (sortVertical == 0 && sortHorizontal == 0)
            {
                if (horizontalFramings != null)
                {
                    horizontalFramings.
                        OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                        ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X);
                }

                if (verticalFramings != null)
                {
                    verticalFramings.
                        OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                        ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y);
                }
            }
            if (sortVertical == 0 && sortHorizontal == 1)
            {
                if (horizontalFramings != null)
                {
                    horizontalFramings.
                        OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                        ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X);
                }

                if (verticalFramings != null)
                {
                    verticalFramings.
                        OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                        ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y);
                }
            }
            if (sortVertical == 1 && sortHorizontal == 0)
            {
                if (horizontalFramings != null)
                {
                    horizontalFramings.
                        OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                        ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X);
                }

                if (verticalFramings != null)
                {
                    verticalFramings.
                        OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                        ThenBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y);
                }
            }
            if (sortVertical == 1 && sortHorizontal == 1)
            {
                if (horizontalFramings != null)
                {
                    horizontalFramings.
                        OrderBy(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y).
                        ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X);
                }

                if (verticalFramings != null)
                {
                    verticalFramings.
                        OrderByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X).
                        ThenByDescending(k => ((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y);
                }
            }
        }

        #endregion
    }
}
