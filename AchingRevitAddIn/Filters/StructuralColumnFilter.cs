#region namespace
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Filter for structural columns
    /// </summary>
    class StructuralColumnFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_StructuralColumns);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
