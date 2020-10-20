#region namespaces
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for SplitWallsUI.xaml
    /// </summary>
    public partial class SplitWallsUI : Window, IDisposable
    {
        public SplitWallsUI()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            this.Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SplitWallsButton_Click(object sender, RoutedEventArgs e)
        {
            double gap;
            int numberElements;
            int divisionType = tabs.SelectedIndex;

            // Split with grids
            if (divisionType == 0)
            {
                if (GapText_Grid.Text != "")
                {
                    gap = double.Parse(GapText_Grid.Text);
                    numberElements = 1;
                    this.Close();
                    SplitWalls.SplitWall(gap, numberElements, divisionType);
                }
                else
                {
                    string message = "Please inform the gap lenght.";
                    MessageBox.Show(message, "Missing information");
                }
            }
            // Split equal parts
            else if (divisionType == 1)
            {
                if (GapText_Equal.Text != "" && Divide_Equal.Text != "")
                {
                    gap = double.Parse(GapText_Equal.Text);
                    numberElements = int.Parse(Divide_Equal.Text);
                    this.Close();
                    SplitWalls.SplitWall(gap, numberElements, divisionType);

                }
                else if (int.Parse(Divide_Equal.Text) == 0)
                {
                    string message = "The number of divisions cannot be 0.";
                    MessageBox.Show(message, "Missing information");
                }
                else if (int.Parse(Divide_Equal.Text) < 0)
                {
                    string message = "The number of divisions cannot be less 0.";
                    MessageBox.Show(message, "Missing information");
                }
                else
                {
                    string message = "Please inform the gap lenght and the number o divisions.";
                    MessageBox.Show(message, "Missing information");
                }
            }
            // Split with grids and equal parts
            else
            {
                if (GapText_GridEqual.Text != "" && Divide_GridEqual.Text != "")
                {
                    gap = double.Parse(GapText_GridEqual.Text);
                    numberElements = int.Parse(Divide_GridEqual.Text);
                    this.Close();
                    SplitWalls.SplitWall(gap, numberElements, divisionType);
                }
                else if (int.Parse(Divide_GridEqual.Text) == 0)
                {
                    string message = "The number of divisions cannot be 0.";
                    MessageBox.Show(message, "Missing information");
                }
                else if (int.Parse(Divide_GridEqual.Text) < 0)
                {
                    string message = "The number of divisions cannot be less 0.";
                    MessageBox.Show(message, "Missing information");
                }
                else
                {
                    string message = "Please inform the gap lenght and the number o divisions.";
                    MessageBox.Show(message, "Missing information");
                }
            }

            
        }
    }
}
