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
using System.Net;
using System.Text.RegularExpressions;
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

                /*
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

                // Using task dialog to show the order of the framings
                string s = "";
                int count = 1;
                foreach (Element elem in sortedFramings)
                {
                    s = s + "Beam: " + count
                        + "\n";

                    XYZ startPoint = ((LocationCurve)elem.Location).Curve.GetEndPoint(0);
                    XYZ middlePoint = ((LocationCurve)elem.Location).Curve.Evaluate(0.5, true);
                    XYZ endPoint = ((LocationCurve)elem.Location).Curve.GetEndPoint(1);
                    XYZ vector = (endPoint - startPoint).Normalize();

                    s = s
                        + " > Start point: " + startPoint.ToString()
                        + "\n"
                        + " > Middle point: " + middlePoint.ToString()
                        + "\n"
                        + " > End point: " + endPoint.ToString()
                        + "\n"
                        + " > Vector: " + vector.ToString()
                        + "\n"
                        + "-------------------------------------------------------"
                        + "\n";

                    count++;
                }
                */

                // group by center point
                List<IGrouping<double, Element>> groupedHorizontalFramings = new List<IGrouping<double, Element>>();
                List<IGrouping<double, Element>> groupedVerticalFramings = new List<IGrouping<double, Element>>();
                List<IGrouping<double, Element>> groupedFramings = new List<IGrouping<double, Element>>();

                if (horizontalFramings != null)
                {
                    groupedHorizontalFramings = GroupHorizontalFramings(horizontalFramings, sortHorizontal, sortVertical);

                    foreach (var group in groupedHorizontalFramings)
                    {
                        groupedFramings.Add(group);
                    }
                }
                if (verticalFramings != null)
                {
                    groupedVerticalFramings = GroupVerticalFramings(verticalFramings, sortHorizontal, sortVertical);

                    foreach (var group in groupedVerticalFramings)
                    {
                        groupedFramings.Add(group);
                    }
                }

                // Get total number of beams
                int totalNumberOfBeams = 0;

                foreach (var group in groupedFramings)
                {
                    foreach (Element elem in group)
                    {
                        totalNumberOfBeams++;
                    }
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Set 'Mark' parameter");

                    // Remove warnings
                    FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                    // Name beams
                    char[] letters = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i).ToArray();

                    foreach (var group in groupedFramings)
                    {
                        if (group.Count() < 2)
                        {
                            Element beam = group.ToList()[0];
                            Parameter mark = beam.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                            GenerateName generatedName = new GenerateName();
                            string name = generatedName.Name(initialNumber, prefix, totalNumberOfBeams);

                            mark.Set(name);
                            initialNumber++;
                        }
                        else
                        {
                            for (int i = 0; i < group.Count(); i++)
                            {
                                // If it is the last item
                                if (i == group.Count() - 1)
                                {

                                }
                                else
                                {
                                    Element currentBeam = group.ToList()[i];
                                    XYZ startPointCB = ((LocationCurve)currentBeam.Location).Curve.GetEndPoint(0);
                                    XYZ endPointCB = ((LocationCurve)currentBeam.Location).Curve.GetEndPoint(1);

                                    Element nextBeam = group.ToList()[i + 1];
                                    XYZ starPointNB = ((LocationCurve)nextBeam.Location).Curve.GetEndPoint(0);
                                    XYZ endPointNB = ((LocationCurve)nextBeam.Location).Curve.GetEndPoint(1);


                                }
                            }
                        }
                    }

                    trans.Commit();
                }

                /*
                string s = "";
                int count = 1;

                foreach (IGrouping<double, Element> group in groupedFramings)
                {
                    s = s + "Key: " + group.Key
                        + "\n";

                    foreach (Element elem in group)
                    {
                        XYZ startPoint = ((LocationCurve)elem.Location).Curve.GetEndPoint(0);
                        XYZ middlePoint = ((LocationCurve)elem.Location).Curve.Evaluate(0.5, true);
                        XYZ endPoint = ((LocationCurve)elem.Location).Curve.GetEndPoint(1);
                        XYZ vector = (endPoint - startPoint).Normalize();

                        s = s
                            + " > Start point: " + startPoint.ToString()
                            + "\n"
                            + " > Middle point: " + middlePoint.ToString()
                            + "\n"
                            + " > End point: " + endPoint.ToString()
                            + "\n"
                            + " > Vector: " + vector.ToString()
                            + "\n"
                            + "-------------------------------------------------------"
                            + "\n";
                    }

                    count++;
                }

                TaskDialog.Show("Framings", s);
                */
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
                sortedHorizontalFramings = horizontalFramings
                    .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ToList();
            }
            if (sortVertical == 0 && sortHorizontal == 1)
            {
                sortedHorizontalFramings = horizontalFramings
                    .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 0)
            {
                sortedHorizontalFramings = horizontalFramings
                    .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 1)
            {
                sortedHorizontalFramings = horizontalFramings
                    .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ToList();
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
                sortedVerticalFramings = verticalFramings
                    .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ToList();
            }
            if (sortVertical == 0 && sortHorizontal == 1)
            {
                sortedVerticalFramings = verticalFramings
                    .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 0)
            {
                sortedVerticalFramings = verticalFramings
                    .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ToList();
            }
            if (sortVertical == 1 && sortHorizontal == 1)
            {
                sortedVerticalFramings = verticalFramings
                    .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                    .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                    .ToList();
            }

            return sortedVerticalFramings;
        }

        /// <summary>
        /// Group horizontal framings
        /// </summary>
        /// <param name="horizontalFramings"></param>
        /// <param name="sortHorizontal"></param>
        /// <param name="sortVertical"></param>
        /// <returns></returns>
        internal static List<IGrouping<double, Element>> GroupHorizontalFramings(IList<Element> horizontalFramings, int sortHorizontal, int sortVertical)
        {
            List<IGrouping<double, Element>> groupedHorizontalFramings = new List<IGrouping<double, Element>>();

            if (sortVertical == 0 && sortHorizontal == 0)
            {
                groupedHorizontalFramings = horizontalFramings
                .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ToList();
            }

            if (sortVertical == 0 && sortHorizontal == 1)
            {
                groupedHorizontalFramings = horizontalFramings
                .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ToList();
            }

            if (sortVertical == 1 && sortHorizontal == 0)
            {
                groupedHorizontalFramings = horizontalFramings
                .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ToList();
            }

            if (sortVertical == 1 && sortHorizontal == 1)
            {
                groupedHorizontalFramings = horizontalFramings
                .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .ToList();
            }

            return groupedHorizontalFramings;
        }

        /// <summary>
        /// Group vertical framings
        /// </summary>
        /// <param name="verticalFramings"></param>
        /// <param name="sortHorizontal"></param>
        /// <param name="sortVertical"></param>
        /// <returns></returns>
        internal static List<IGrouping<double, Element>> GroupVerticalFramings(IList<Element> verticalFramings, int sortHorizontal, int sortVertical)
        {
            List<IGrouping<double, Element>> groupedVerticalFramings = new List<IGrouping<double, Element>>();

            if (sortVertical == 0 && sortHorizontal == 0)
            {
                groupedVerticalFramings = verticalFramings
                .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ToList();
            }

            if (sortVertical == 0 && sortHorizontal == 1)
            {
                groupedVerticalFramings = verticalFramings
                .OrderBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ToList();
            }

            if (sortVertical == 1 && sortHorizontal == 0)
            {
                groupedVerticalFramings = verticalFramings
                .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ThenBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ToList();
            }

            if (sortVertical == 1 && sortHorizontal == 1)
            {
                groupedVerticalFramings = verticalFramings
                .OrderByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ThenByDescending(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).Y, 5))
                .GroupBy(k => Math.Round(((LocationCurve)k.Location).Curve.Evaluate(0.5, true).X, 5))
                .ToList();
            }

            return groupedVerticalFramings;
        }

        #endregion
    }
}
