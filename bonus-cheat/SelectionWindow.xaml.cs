using System.Text;
using System.Windows;

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
