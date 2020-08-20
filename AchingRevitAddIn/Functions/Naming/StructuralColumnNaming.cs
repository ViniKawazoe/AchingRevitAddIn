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
                //Selection selection = uiapp.ActiveUIDocument.Selection;
                //IList<Reference> strColumns = selection.PickObjects(ObjectType.Element, new StrColumnPickFilter(), "Select columns to name");

                //strColumns = strColumns.OrderByDescending(x => x.GlobalPoint.Y).ToList();
                //int cont = strColumns.Count();

                var columnCollector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
                columnCollector.OfCategory(BuiltInCategory.OST_StructuralColumns);
                IList<Element> columnList = columnCollector.ToElements();
                columnList = columnList.OrderByDescending(x => x.Location).ToList();


                foreach(Element column in columnList)
                {
                    Parameter columnMark = column.LookupParameter("Mark");
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
