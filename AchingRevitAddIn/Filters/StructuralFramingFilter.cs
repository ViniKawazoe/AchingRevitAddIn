#region namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace AchingRevitAddIn.Filters
{
    /// <summary>
    /// Filter the Structural Framing
    /// </summary>
    class StructuralFramingFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_StructuralFraming);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
