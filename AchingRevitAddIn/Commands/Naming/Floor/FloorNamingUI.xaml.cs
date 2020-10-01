#region namespaces
using System;
using System.Windows;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for FloorNamingUI.xaml
    /// </summary>
    public partial class FloorNamingUI : Window, IDisposable
    {
        public FloorNamingUI()
        {
            InitializeComponent();
        }

        private void SelectFloorsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrefixText.Text != "" && InitialNumberText.Text != "")
            {
                // Sets the prefix of the floors names
                string prefix = PrefixText.Text;
                // Sets the initial number of the floors names
                int initialNumber = int.Parse(InitialNumberText.Text);
                // Sets the order to which the method will organize the names
                int indexVertical = SortVertical.SelectedIndex;
                int indexHorizontal = SortHorizontal.SelectedIndex;

                this.Close();
                FloorNaming.NameFloors(prefix, initialNumber, indexVertical, indexHorizontal);
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
