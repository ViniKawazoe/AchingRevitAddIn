using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AchingRevitAddIn.Filters.ElementFilter;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AchingRevitAddIn.Functions.Naming
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class StructuralColumnNaming : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get application and document object
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {
                Selection selection = uiapp.ActiveUIDocument.Selection;
                IList<Reference> strColumns = selection.PickObjects(ObjectType.Element, new StrColumnPickFilter(), "Select columns to name");

                foreach(Reference element in strColumns)
                {
                    XYZ globalPoint = element.GlobalPoint;

                }

                //Transaction trans = new Transaction(doc);
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

            return Result.Succeeded;
        }
    }
}
