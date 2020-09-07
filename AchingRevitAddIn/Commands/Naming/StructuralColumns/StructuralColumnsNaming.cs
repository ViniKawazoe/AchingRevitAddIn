#region namespaces
using System;
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
    class StructuralColumnsNaming : IExternalCommand
    {
        private static UIDocument Uidoc { get; set; }

        #region public methods
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Document doc = uidoc.Document;

            // Send it to the public variable so other methods can call it
            Uidoc = uidoc;

            // Only allow view plans, 3D views, sections and elevations
            if (uidoc.ActiveView as ViewPlan == null &&
                uidoc.ActiveView as View3D == null &&
                uidoc.ActiveView as ViewSection == null)
            {
                return Result.Failed;
            }

            StructuralColumnNamingUI structuralColumnNamingWindow = new StructuralColumnNamingUI();
            structuralColumnNamingWindow.ShowDialog();

            return Result.Succeeded;
        }

        /// <summary>
        /// Create a "Mark" parameter for each of the selected structural column
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="prefix"></param>
        /// <param name="initialNumber"></param>
        static internal void NameColumns(string prefix, int initialNumber, int sortIndex, bool replicate)
        {
            try
            {
                // Get Document
                UIDocument uidoc = Uidoc;
                Document doc = uidoc.Document;

                // Create filter
                StructuralColumnFilter filter = new StructuralColumnFilter();

                // Apply filter and select multiple structural columns
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select structural columns");

                // Convert References to Elements
                IList<Element> strColumns = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                IList<Element> sortedStrColumns = null;

                // Order the structural columns by location
                if (sortIndex == 0)
                {
                    sortedStrColumns = strColumns.OrderByDescending(x => (x.Location as LocationPoint).Point.Y)
                        .ThenBy(x => (x.Location as LocationPoint).Point.X).ToList();
                }
                if (sortIndex == 1)
                {
                    sortedStrColumns = strColumns.OrderByDescending(x => (x.Location as LocationPoint).Point.Y)
                        .ThenByDescending(x => (x.Location as LocationPoint).Point.X).ToList();
                }
                if (sortIndex == 2)
                {
                    sortedStrColumns = strColumns.OrderBy(x => (x.Location as LocationPoint).Point.Y)
                        .ThenBy(x => (x.Location as LocationPoint).Point.X).ToList();
                }
                if (sortIndex == 3)
                {
                    sortedStrColumns = strColumns.OrderBy(x => (x.Location as LocationPoint).Point.Y)
                        .ThenByDescending(x => (x.Location as LocationPoint).Point.X).ToList();
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Set 'Mark' parameter");

                    foreach (Element strColumn in sortedStrColumns)
                    {
                        Parameter p = strColumn.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                        GenerateName generatedName = new GenerateName();
                        string name = generatedName.Name(initialNumber, prefix, sortedStrColumns.Count);

                        p.Set(name);
                        initialNumber++;
                    }

                    trans.Commit();
                }

                if (replicate)
                {

                }
            }
            catch
            {
            }
        }

        static internal void ReplicateToAlignedColumns()
        {

        }
        #endregion
    }
}
