#region namespaces
using System.Windows;
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
            if (PrefixText.Text != "" && InitialNumberText.Text != "")
            {
                ///Sets the prefix of the columns names
                string prefix = PrefixText.Text;
                ///Sets the initial number of the columns names
                int initialNumber = int.Parse(InitialNumberText.Text);
                ///Sets the order to which the method will organize the names
                int index = SortDropdown.SelectedIndex;
                ///Choose if the created names are going to be replicated to the aligned columns
                bool checkbox = (bool) ReplicateCheckBox.IsChecked;

                this.Close();
                StructuralColumnsNaming.NameColumns(prefix, initialNumber, index, checkbox);
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
