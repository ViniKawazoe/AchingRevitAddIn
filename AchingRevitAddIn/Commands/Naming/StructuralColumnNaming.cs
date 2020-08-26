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
                IList<Reference> strColumns = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select structural columns");

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
