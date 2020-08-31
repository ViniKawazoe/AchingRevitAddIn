#region namespaces
using System.Windows.Media.Imaging;
#endregion

/// <summary>
/// This code was adapted from the Revit API - Plugin Essentials series, by Marko Koljancic
/// </summary>
namespace AchingRevitAddIn.Resources
{
    /// <summary>
    /// Gets the embedded resource image from the AchingRevitAddIn assembly based on user provided file name with extension
    /// </summary>
    public static class ResourceImage
    {
        #region public methods

        /// <summary>
        /// Gets the icon image from the resource assembly
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BitmapImage GetIcon(string name)
        {
            // Create the resource reader stream
            var stream = ResourceAssembly.GetAssembly().GetManifestResourceStream(ResourceAssembly.GetNamespace() + "Images.Icons." + name);
            var image = new BitmapImage();

            // Construct and return image
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            // Return constructed BitmapImage
            return image;
        }

        #endregion
    }
}
