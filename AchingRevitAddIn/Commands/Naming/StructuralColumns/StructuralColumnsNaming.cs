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
    class StructuralColumnsNaming : IExternalCommand
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
        static internal void NameColumns(string prefix, int initialNumber, int sortVertical, int sortHorizontal, bool replicate)
        {
            try
            {
                // Get UIDocument and Document
                UIDocument uidoc = Uidoc;
                Document doc = uidoc.Document;

                // Create filter
                StructuralColumnFilter filter = new StructuralColumnFilter();

                // Apply filter and select multiple structural columns
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select structural columns");

                // Convert References to Elements
                IList<Element> strColumns = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                IList<Element> sortedStrColumns = null;

                // Sort the structural columns by location
                if (sortVertical == 0 && sortHorizontal == 0)
                {
                    sortedStrColumns = strColumns
                        .OrderByDescending(x => Math.Round((x.Location as LocationPoint).Point.Y, 5))
                        .ThenBy(x => Math.Round((x.Location as LocationPoint).Point.X, 5))
                        .ToList();
                }
                if (sortVertical == 0 && sortHorizontal == 1)
                {
                    sortedStrColumns = strColumns
                        .OrderByDescending(x => Math.Round((x.Location as LocationPoint).Point.Y, 5))
                        .ThenByDescending(x => Math.Round((x.Location as LocationPoint).Point.X, 5))
                        .ToList();
                }
                if (sortVertical == 1 && sortHorizontal == 0)
                {
                    sortedStrColumns = strColumns
                        .OrderBy(x => Math.Round((x.Location as LocationPoint).Point.Y, 5))
                        .ThenBy(x => Math.Round((x.Location as LocationPoint).Point.X, 5))
                        .ToList();
                }
                if (sortVertical == 1 && sortHorizontal == 1)
                {
                    sortedStrColumns = strColumns
                        .OrderBy(x => Math.Round((x.Location as LocationPoint).Point.Y, 5))
                        .ThenByDescending(x => Math.Round((x.Location as LocationPoint).Point.X, 5))
                        .ToList();
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

                    foreach (Element strColumn in sortedStrColumns)
                    {
                        Parameter mark = strColumn.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                        GenerateName generatedName = new GenerateName();
                        string name = generatedName.Name(initialNumber, prefix, sortedStrColumns.Count);

                        mark.Set(name);
                        initialNumber++;
                    }

                    trans.Commit(failureHandlingOptions);
                }

                if (replicate)
                {
                    foreach (Element column in sortedStrColumns)
                    {
                        ReplicateAbove(column);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Replicate the 'Mark' parameter from the structural column to the aligned columns above
        /// </summary>
        /// <param name="structuralColumn"></param>
        static internal void ReplicateAbove(Element structuralColumn)
        {
            // Get UIDocument and Document
            UIDocument uidoc = Uidoc;
            Document doc = uidoc.Document;

            Element referenceColumn = structuralColumn;

            while (referenceColumn != null)
            {
                Element newReferenceColumn = null;

                // Get reference columns location point and 'Mark' parameter
                XYZ referenceColumnLocation = (referenceColumn.Location as LocationPoint).Point;
                string referenceColumnMark = referenceColumn.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();

                // Get column and top level
                Parameter columnTopLevel = referenceColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);

                // Get all columns on the top level
                ElementLevelFilter levelFilter = new ElementLevelFilter(columnTopLevel.AsElementId());
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> allColumnsOnTopLevel = collector.OfCategory(BuiltInCategory.OST_StructuralColumns).WherePasses(levelFilter).ToElements();

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Replicate 'Mark' parameter");

                    // Remove warnings
                    FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                    if (allColumnsOnTopLevel != null)
                    {

                        foreach (Element column in allColumnsOnTopLevel)
                        {
                            XYZ columnLocation = (column.Location as LocationPoint).Point;
                            Parameter p = column.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                            if (columnLocation.IsAlmostEqualTo(referenceColumnLocation, 0.001))
                            {
                                p.Set(referenceColumnMark);
                                newReferenceColumn = column;
                                break;
                            }
                        }

                        referenceColumn = newReferenceColumn;
                    }
                    else
                    {
                        referenceColumn = null;
                    }

                    trans.Commit(failureHandlingOptions);
                }
            }
        }
        #endregion
    }
}
