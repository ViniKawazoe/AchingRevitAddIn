#region namespaces
using System;
using System.Windows;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for StructuralFramingNamingUI.xaml
    /// </summary>
    public partial class StructuralFramingNamingUI : Window, IDisposable
    {
        public StructuralFramingNamingUI()
        {
            InitializeComponent();
        }

        private void SelectFramingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrefixText.Text != "" && InitialNumberText.Text != "")
            {
                ///Sets the prefix of the beams names
                string prefix = PrefixText.Text;
                ///Sets the initial number of the beams names
                int initialNumber = int.Parse(InitialNumberText.Text);
                ///Sets the order to which the method will organize the names
                int indexVertical = SortVertical.SelectedIndex;
                int indexHorizontal = SortHorizontal.SelectedIndex;

                this.Close();
                StructuralFramingNaming.NameFramings(prefix, initialNumber, indexVertical, indexHorizontal);
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

        public void Dispose()
        {
            this.Close();
        }
    }
}
