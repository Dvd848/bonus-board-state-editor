using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                this.Refresh();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                MessageBox.Show("שגיאה בהתחברות למשחק!");
                throw;
            }

        }

        private void Refresh()
        {
            InitializeScrabbleBoard();
            PopulateRacks();
        }

        private void InitializeScrabbleBoard()
        {
            scrabbleBoard.Children.Clear();
            char[,] board = this.bonus.GetBoard();
            
            for (int row = 0; row < Bonus.BONUS_BOARD_DIMENTION; row++)
            {
                for (int col = 0; col < Bonus.BONUS_BOARD_DIMENTION; col++)
                {
                    char letter = board[row, col].ToString()[0];

                    UIElement child;
                    Border cellBorder;

                    switch (letter)
                    {
                        case ((char)Bonus.SpecialChars.Space):
                            cellBorder = new Border
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
                            break;
                        case ((char)Bonus.SpecialChars.Blocked):
                            Border blockedCell;
                            if (row == 0 || row == Bonus.BONUS_BOARD_DIMENTION - 1 || col == 0 || col == Bonus.BONUS_BOARD_DIMENTION - 1)
                            {
                                blockedCell = new Border
                                {
                                    Background = Brushes.White,
                                    BorderBrush = Brushes.White,
                                    BorderThickness = new Thickness(1)
                                };
                            }
                            else
                            {
                                blockedCell = new Border
                                {
                                    BorderBrush = Brushes.Gray,
                                    BorderThickness = new Thickness(1),
                                    Background = new SolidColorBrush(Colors.PeachPuff),
                                };
                            }
                            child = blockedCell;
                            break;
                        default:
                            cellBorder = new Border
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
                            break;
                    }
                    
                    Grid.SetRow(child, row);
                    Grid.SetColumn(child, col);
                    scrabbleBoard.Children.Add(child);
                }
            }
        }

        private void PopulateRacks()
        {
            foreach (Bonus.Entity entity in new Bonus.Entity[] {Bonus.Entity.Player1, Bonus.Entity.Player2})
            {
                StackPanel rack = GetRackForEntity(entity);
                rack.Children.Clear();

                char[] letters = this.bonus.GetEntityLetters(entity);
                if (entity == Bonus.Entity.Rack)
                {
                    Array.Reverse(letters);
                }
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

                    rack.Children.Add(tileButton);
                }
            }
        }

        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var row = Grid.GetRow((UIElement)button.Parent);
            var col = Grid.GetColumn((UIElement)button.Parent);

            string? content = button.Content?.ToString();
            char letter = (content == null) ? '?' : content[0];
            
            ShowBoardSelectionWindow(row, col, letter);
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var stack = (StackPanel)button.Parent;
            Bonus.Entity entity;
            int index = stack.Children.IndexOf(sender as UIElement);

            if (stack == leftPlayerRack)
            {
                entity = Bonus.Entity.Player1;
            }
            else if (stack == rightPlayerRack)
            {
                entity = Bonus.Entity.Player2;
            }
            else
            {
                return;
            }

            ShowRackSelectionWindow(entity, index, this.bonus.GetEntityLetters(entity)[index]);
        }

        private StackPanel GetRackForEntity(Bonus.Entity entity)
        {
            return new Dictionary<Bonus.Entity, StackPanel>
            {
                { Bonus.Entity.Player1, leftPlayerRack },
                { Bonus.Entity.Player2, rightPlayerRack }
            }[entity];
        }

        private void ShowRackSelectionWindow(Bonus.Entity entity, int index, char letter)
        {
            SelectionWindow selectionWindow = new SelectionWindow(letter, this.bonus.Letters);
            selectionWindow.ShowDialog();

            if (selectionWindow.DialogResult.HasValue && selectionWindow.DialogResult.Value)
            {
                string? selectedOption = selectionWindow.letterSelectionComboBox.SelectedItem?.ToString();
                if (selectedOption == null)
                {
                    return;
                }

                this.bonus.SetEntityLetter(entity, index, selectedOption[0]);
                this.Refresh();
            }
            
        }

        private void ShowBoardSelectionWindow(int row, int col, char letter)
        {
            SelectionWindow selectionWindow = new SelectionWindow(letter, this.bonus.Letters);
            selectionWindow.ShowDialog();

            if (selectionWindow.DialogResult.HasValue && selectionWindow.DialogResult.Value)
            {
                string? selectedOption = selectionWindow.letterSelectionComboBox.SelectedItem?.ToString();
                if (selectedOption == null)
                {
                    return;
                }

                this.bonus.SetBoardLetter(row, col, selectedOption[0]);
                this.Refresh();
            }

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs evt)
        {
            try
            {
                this.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show("שגיאה!");
            }

        }

        private void Menu_ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Menu_ClickAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}


