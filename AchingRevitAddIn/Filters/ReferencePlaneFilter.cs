#region namespace
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Filter reference planes
    /// </summary>
    class ReferencePlaneFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_CLines);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
