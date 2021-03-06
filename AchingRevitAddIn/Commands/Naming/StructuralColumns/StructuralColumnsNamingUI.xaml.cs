﻿#region namespaces
using System;
using System.Windows;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for StructuralColumnNamingUI.xaml
    /// </summary>
    public partial class StructuralColumnNamingUI : Window, IDisposable
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
                int indexVertical = SortVertical.SelectedIndex;
                int indexHorizontal = SortHorizontal.SelectedIndex;
                ///Choose if the created names are going to be replicated to the aligned columns
                bool checkbox = (bool) ReplicateCheckBox.IsChecked;

                this.Close();
                StructuralColumnsNaming.NameColumns(prefix, initialNumber, indexVertical, indexHorizontal, checkbox);
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
