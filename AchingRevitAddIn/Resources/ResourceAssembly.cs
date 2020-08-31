#region namespaces
using System.Reflection;
#endregion

/// <summary>
/// This code was adapted from the Revit API - Plugin Essentials series, by Marko Koljancic
/// </summary>
namespace AchingRevitAddIn.Resources
{
    /// <summary>
    /// Resource assembly helper methods
    /// </summary>
    public static class ResourceAssembly
    {
        #region public methods

        /// <summary>
        /// Gets the currently resource assembly
        /// </summary>
        /// <returns></returns>
        public static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        public static Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// Gets the namespace of the currenttly running resource assembly
        /// </summary>
        /// <returns></returns>
        public static string GetNamespace()
        {
            return typeof(ResourceAssembly).Namespace + ".";
        }

        #endregion
    }
}
