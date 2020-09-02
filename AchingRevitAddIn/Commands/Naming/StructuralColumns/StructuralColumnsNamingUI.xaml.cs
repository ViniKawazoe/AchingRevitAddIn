#region namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = Autodesk.Revit.ApplicationServices.Application;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for StructuralColumnNamingUI.xaml
    /// </summary>
    public partial class StructuralColumnNamingUI : Window
    {
        public StructuralColumnNamingUI()
        {
            InitializeComponent();
        }

        private void SelectColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PrefixText.Text != "" && this.InitialNumberText.Text != "")
            {
                StructuralColumnsNaming.NameColumns(this.PrefixText.Text, int.Parse(this.InitialNumberText.Text));
            }
            else
            {
                string message = "Please select a prefix and initial number.";
                MessageBox.Show(message, "Missing information");
            }
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
