#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using AchingRevitAddIn.Commands.Naming.StructuralColumnNaming;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Create a "Mark" parameter for each of the selected structural column
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class StructuralColumnNaming : IExternalCommand
    {
        #region public method

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Only allow view plans, 3D views, sections and elevations
            if(uidoc.ActiveView as ViewPlan == null &&
                uidoc.ActiveView as View3D == null &&
                uidoc.ActiveView as ViewSection == null)
            {
                return Result.Failed;
            }

            try
            {
                StructuralColumnNamingUI structuralColumnNamingUI = new StructuralColumnNamingUI();
                var res = structuralColumnNamingUI.ShowDialog();

                // Create filter
                StructuralColumnFilter filter = new StructuralColumnFilter();

                // Apply filter and select multiple structural columns
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select structural columns");

                // Convert References to Elements
                IList<Element> strColumns = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                // Order the structural columns by location
                IList<Element> sortedStrColumns = strColumns.OrderByDescending(x => (x.Location as LocationPoint).Point.Y)
                    .ThenBy(x => (x.Location as LocationPoint).Point.X).ToList();

                // Set the initial number of the structural column
                int count = 1;

                // Set the prefix of the structural column
                string prefix = "P";

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Set 'Mark' parameter");

                    foreach (Element strColumn in sortedStrColumns)
                    {
                        Parameter p = strColumn.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                        GenerateName generatedName = new GenerateName();
                        string name = generatedName.Name(count, prefix, sortedStrColumns.Count);

                        p.Set(name);
                        count++;
                    }

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        //public void ShowUI


        #endregion
    }
}
