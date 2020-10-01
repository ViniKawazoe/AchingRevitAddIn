#region namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace AchingRevitAddIn.Filters
{
    /// <summary>
    /// Filter the floors
    /// </summary>
    class FloorFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Floors);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
