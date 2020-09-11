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

                IList<Element> horizontalFramings = null;
                IList<Element> verticalFramings = null;

                // Separate horizontal and vertical beams 
                foreach (Element beam in strFramings)
                {
                    IList<Curve> curves = new List<Curve>();
                    XYZ startPoint = ((LocationCurve)beam.Location).Curve.GetEndPoint(0);
                    XYZ endPoint = ((LocationCurve)beam.Location).Curve.GetEndPoint(1);

                    XYZ vector = (endPoint - startPoint).Normalize();

                    if (vector.X >= vector.Y)
                    {
                        horizontalFramings.Add(beam);
                    }
                    else
                    {
                        verticalFramings.Add(beam);
                    }
                }

                // Grouping the beams
                if (horizontalFramings != null)
                {
                    // Group the continuous beams
                }

                if (verticalFramings != null)
                {
                    // Group the continuous beams
                }
            }
            catch
            {
            }
        }

        static internal IList<Element> GroupBeams(IList<Element> beamList)
        {
            IList<Element> list = null;
            return list;
        }

        #endregion
    }
}
