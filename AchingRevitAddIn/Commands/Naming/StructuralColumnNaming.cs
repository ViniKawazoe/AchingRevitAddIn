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

            try
            {
                // Create filter
                StructuralColumnFilter filter = new StructuralColumnFilter();

                // Apply filter and select multiple structural columns
                IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select structural columns");

                // Convert References to Elements
                IList<Element> strColumns = pickedReferences.Select(x => doc.GetElement(x)).ToList();

                // Order the structural columns by location
                IList<Element> sortedStrColumns = strColumns.OrderByDescending(x => x.GetAnalyticalModel().GetCurve().GetEndPoint(0).Y)
                    .ThenBy(x => x.GetAnalyticalModel().GetCurve().GetEndPoint(0).X).ToList();

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

                        if (sortedStrColumns.Count < 10)
                        {
                            p.Set(prefix + count);
                        }
                        if (sortedStrColumns.Count >= 10 && sortedStrColumns.Count < 100)
                        {
                            if (count < 10)
                            {
                                p.Set(prefix + "0" + count);
                            }
                            else
                            {
                                p.Set(prefix + count);
                            }
                        }
                        if (sortedStrColumns.Count >= 100 && sortedStrColumns.Count < 1000)
                        {
                            if (count < 10)
                            {
                                p.Set(prefix + "00" + count);
                            }
                            else if (count >= 10 && count < 100)
                            {
                                p.Set(prefix + "0" + count);
                            }
                            else
                            {
                                p.Set(prefix + count);
                            }
                        }

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
        #endregion
    }
}
