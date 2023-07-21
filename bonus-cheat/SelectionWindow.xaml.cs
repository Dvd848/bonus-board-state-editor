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
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var letter in letters)
            {
                stringBuilder.Append(letter);
            }

            changeLetterLabel.Content = $"החלפת האות '{originalLetter}' באות:";

            string hebrewLetters = stringBuilder.ToString();
            char[] lettersArray = hebrewLetters.ToCharArray();

            letterSelectionComboBox.ItemsSource = lettersArray;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
