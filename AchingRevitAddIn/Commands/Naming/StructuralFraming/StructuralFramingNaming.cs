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

                Options Op = app.Create.NewGeometryOptions();
                Op.ComputeReferences = true;
                Op.IncludeNonVisibleObjects = true;
                Op.View = doc.ActiveView;

                string s = "";
                int count = 1;
                // Separate horizontal and vertical beams 
                foreach (Element b in strFramings)
                {
                    IList<Curve> curves = new List<Curve>();
                    XYZ startPoint = ((LocationCurve)b.Location).Curve.GetEndPoint(0);
                    XYZ endPoint = ((LocationCurve)b.Location).Curve.GetEndPoint(1);

                    
                    s = s + "Beam " + count 
                        + "\n"
                        + "Start point: " + startPoint.ToString()
                        + "\n"
                        + "End point: " + endPoint.ToString()
                        + "\n"
                        + "------------------------------------------"
                        + "\n";

                    count++;
                }
                TaskDialog.Show("Points", s);
            }
            catch
            {
            }
        }

        #endregion
    }
}
