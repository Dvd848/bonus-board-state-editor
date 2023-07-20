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

namespace bonus_cheat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bonus bonus;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                this.bonus = new Bonus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
                // TODO: Handle
            }

            InitializeScrabbleBoard();

            PopulateTileRack();

            PopulatePlayerRacks();
        }

        private void InitializeScrabbleBoard()
        {
            char[,] board = this.bonus.getBoard();
            
            for (int row = 0; row < Bonus.BONUS_BOARD_DIMENTION; row++)
            {
                for (int col = 0; col < Bonus.BONUS_BOARD_DIMENTION; col++)
                {
                    string letter = board[row, col].ToString();

                    UIElement child;

                    if (letter == " ")
                    {
                        Border cellBorder = new Border
                        {
                            BorderBrush = Brushes.Gray,
                            BorderThickness = new Thickness(1)
                        };

                        TextBlock cellTextBlock = new TextBlock
                        {
                            Text = "",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 18
                        };
                        cellBorder.Child = cellTextBlock;

                        child = cellBorder;
                    }
                    else if (letter == "#")
                    {
                        Border whiteCell = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.White,
                            BorderThickness = new Thickness(1)
                        };
                        child = whiteCell;
                    }
                    else
                    {
                        Border cellBorder = new Border
                        {
                            BorderBrush = Brushes.Gray,
                            BorderThickness = new Thickness(1)
                        };

                        Button cellButton = new Button
                        {
                            Content = letter,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        };

                        cellButton.Click += BoardButton_Click;

                        cellBorder.Child = cellButton;
                        child = cellBorder;
                    }

                    Grid.SetRow(child, row);
                    Grid.SetColumn(child, col);
                    scrabbleBoard.Children.Add(child);
                }
            }
        }

        private void PopulateTileRack()
        {

            char[] rack = this.bonus.getRackLetters();
            for (int i = rack.Length - 1; i >= 0; i--)
            {
                Button tileButton = new Button
                {
                    Content = rack[i],
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5)
                };

                tileButton.Click += TileButton_Click;

                tileRack.Children.Add(tileButton);
            }
        }

        private void PopulatePlayerRacks()
        {
            foreach (Bonus.Player player in (Bonus.Player[])Enum.GetValues(typeof(Bonus.Player)))
            {
                char[] letters = this.bonus.getPlayerLetters(player);
                for (int i = 0; i < letters.Length; i++)
                {
                    Button tileButton = new Button
                    {
                        Content = letters[i],
                        Width = 40,
                        Height = 40,
                        Margin = new Thickness(5)
                    };

                    tileButton.Click += TileButton_Click;

                    getRackForPlayer(player).Children.Add(tileButton);
                }
            }
        }

        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var row = Grid.GetRow((UIElement)button.Parent);
            var col = Grid.GetColumn((UIElement)button.Parent);
            MessageBox.Show($"Button clicked at Row: {row}, Column: {col}");
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var stack = (StackPanel)button.Parent;

            MessageBox.Show("Selected " + stack.Children.IndexOf(sender as UIElement));
        }

        private StackPanel getRackForPlayer(Bonus.Player player)
        {
            return new Dictionary<Bonus.Player, StackPanel>
            {
                { Bonus.Player.Player1, leftPlayerRack },
                { Bonus.Player.Player2, rightPlayerRack }
            }[player];
        }
    }

}
