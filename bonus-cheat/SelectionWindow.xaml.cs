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
using System.Windows.Shapes;

namespace bonus_cheat
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window
    {
        public SelectionWindow(char originalLetter, char[] letters)
        {
            InitializeComponent();

            PopulateSelectionBoxWithHebrewLetters(originalLetter, letters);
        }

        private void PopulateSelectionBoxWithHebrewLetters(char originalLetter, char[] letters)
        {
            // Create a StringBuilder to store the Hebrew letters
            StringBuilder stringBuilder = new StringBuilder();

            // Add Hebrew letters from Unicode values (0x05D0 to 0x05EA) to the StringBuilder
            foreach (var letter in letters)
            {
                // Append the Hebrew letter to the StringBuilder
                stringBuilder.Append(letter);
            }

            changeLetterLabel.Content = $"החלפת האות '{originalLetter}' באות:";

            // Convert the StringBuilder content to a string and split it into individual letters
            string hebrewLetters = stringBuilder.ToString();
            char[] lettersArray = hebrewLetters.ToCharArray();

            // Populate the selectionComboBox with the Hebrew letters
            letterSelectionComboBox.ItemsSource = lettersArray;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the DialogResult to true to indicate that the OK button was clicked
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the DialogResult to false to indicate that the Cancel button was clicked
            DialogResult = false;
            Close();
        }
    }
}
