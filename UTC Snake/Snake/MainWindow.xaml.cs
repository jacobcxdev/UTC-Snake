using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Reflection;

namespace Snake {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        class Settings {
            public int PlayAreaSize;
            public int PlaySquareSize;
            public int FruitSpawnAmount;
        }

        class Position {
            public int X;
            public int Y;
        }

        Settings settings = new Settings() {
            PlayAreaSize = 25,
            PlaySquareSize = 20
        };

        DispatcherTimer timer = new DispatcherTimer() {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        Dictionary<string, List<List<Position>>> icons = new Dictionary<string, List<List<Position>>> { };
        Dictionary<string, ContentControl> iconsOrig = new Dictionary<string, ContentControl>();

        List<Position> playerPos = new List<Position>();
        List<Position> fruitPos = new List<Position>();


        bool settingsBarIsLoaded = false;
        bool needsReload = false;
        bool barIsHidden = true;
        bool isInGame = false;
        bool isPaused = false;
        bool isCountingDown = false;
        int pendingPlayAreaSize = 0;
        int pendingPlaySquareSize = 0;
        int pendingDirection = 0;
        int direction = 0;
        int score = 0;
        int highscore = 0;


        private IEnumerable<ContentControl> GetSquare(Position position) {
            var currentSquare = playArea.Children
                                 .OfType<ContentControl>()
                                 .Where(z => z.Name.StartsWith(String.Format("playSquare_{0}_{1}_", position.X, position.Y)));

            return currentSquare;
        }

        private void SetSquareColor(Position position, SolidColorBrush color) {
            var currentSquare = GetSquare(position);

            foreach (var label in currentSquare) {
                label.Background = color;
            }
        }

        private void SetSquareContent(Position position, object content, HorizontalAlignment hAlign = HorizontalAlignment.Center, VerticalAlignment vAlign = VerticalAlignment.Center) {
            var currentSquare = GetSquare(position);

            foreach (var playSquare in currentSquare) {
                playSquare.HorizontalContentAlignment = hAlign;
                playSquare.VerticalContentAlignment = vAlign;

                playSquare.Content = content;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            playArea.Children.Clear();
            LoadPlayArea();
            LoadIcons();

            timer.Tick += TimerEvent;

            UpdateScore();
            UpdatePlayAreaSize();
            UpdatePlaySquareSize();
            UpdateSpeed();
            UpdateFruitSpawnAmount();

            playPauseButton.Focus();

            ShowHideBar(true);
        }

        private void LoadPlayArea() {
            playArea.Children.Clear();
            for (int i = 0; i < settings.PlayAreaSize; i++)
                for (int j = 0; j < settings.PlayAreaSize; j++) {
                    ColumnDefinition column = new ColumnDefinition {
                        Width = GridLength.Auto
                    };
                    RowDefinition row = new RowDefinition {
                        Height = GridLength.Auto
                    };

                    Label playSquare = new Label {
                        Width = settings.PlaySquareSize,
                        Height = settings.PlaySquareSize,
                        Background = new SolidColorBrush(Colors.Black),
                        Name = String.Format("playSquare_{0}_{1}_", i, j),
                        Padding = new Thickness(0),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = settings.PlaySquareSize * 0.6
                    };

                    playArea.ColumnDefinitions.Add(column);
                    playArea.RowDefinitions.Add(row);
                    playArea.Children.Add(playSquare);

                    Grid.SetColumn(playSquare, i);
                    Grid.SetRow(playSquare, j);
                }
        }

        private void LoadIcons() {
            Dictionary<int, int[]> one = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 0, 1, 0, 0 } },
                { 1, new int[] { 0, 1, 1, 0, 0 } },
                { 2, new int[] { 1, 0, 1, 0, 0 } },
                { 3, new int[] { 0, 0, 1, 0, 0 } },
                { 4, new int[] { 1, 1, 1, 1, 1 } },
            };

            Dictionary<int, int[]> two = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 1, 1, 1, 0 } },
                { 1, new int[] { 1, 0, 0, 0, 1 } },
                { 2, new int[] { 0, 0, 1, 1, 0 } },
                { 3, new int[] { 0, 1, 0, 0, 0 } },
                { 4, new int[] { 1, 1, 1, 1, 1 } },
            };

            Dictionary<int, int[]> three = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 1, 1, 1, 0 } },
                { 1, new int[] { 1, 0, 0, 0, 1 } },
                { 2, new int[] { 0, 0, 1, 1, 0 } },
                { 3, new int[] { 1, 0, 0, 0, 1 } },
                { 4, new int[] { 0, 1, 1, 1, 0 } },
            };

            Dictionary<int, int[]> pause = new Dictionary<int, int[]> {
                { 0, new int[] { 1, 1, 0, 1, 1 } },
                { 1, new int[] { 1, 1, 0, 1, 1 } },
                { 2, new int[] { 1, 1, 0, 1, 1 } },
                { 3, new int[] { 1, 1, 0, 1, 1 } },
                { 4, new int[] { 1, 1, 0, 1, 1 } },
            };

            Dictionary<int, int[]> sad = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 0, 0, 0, 0, 0, 0 } },
                { 1, new int[] { 0, 1, 1, 0, 1, 1, 0 } },
                { 2, new int[] { 0, 0, 0, 0, 0, 0, 1 } },
                { 3, new int[] { 0, 0, 0, 0, 0, 0, 0 } },
                { 4, new int[] { 0, 1, 1, 1, 1, 1, 0 } },
                { 5, new int[] { 0, 1, 0, 0, 0, 1, 0 } },
                { 6, new int[] { 0, 1, 0, 0, 0, 1, 0 } },
            };

            AddIcon(one, "1");
            AddIcon(two, "2");
            AddIcon(three, "3");
            AddIcon(pause, "pause");
            AddIcon(sad, "sad");
        }

        private void AddIcon(Dictionary<int, int[]> icon, string name) {
            icons[name] = new List<List<Position>> { new List<Position>(), new List<Position>() };

            for (int i = 0; i < icon.Count; i++)
                for (int j = 0; j < icon.Count; j++) {
                    Position position = new Position {
                        X = settings.PlayAreaSize / 2 - icon.Count / 2 + j,
                        Y = settings.PlayAreaSize / 2 - icon.Count / 2 + i
                    };
                    switch (icon[i][j]) {
                        case (0):
                            icons[name][0].Add(position);
                            break;
                        case (1):
                            icons[name][1].Add(position);
                            break;
                    }
                }
        }

        private void ShowHideBar(bool hide = false) {
            if (hide || !barIsHidden) {
                bar.RowDefinitions[1].Height = new GridLength(0);
                bar.RowDefinitions[2].Height = new GridLength(0);
                barIsHidden = true;
                if (needsReload)
                    LoadPlayArea();
                needsReload = false;
            } else if (barIsHidden){
                bar.RowDefinitions[1].Height = GridLength.Auto;
                bar.RowDefinitions[2].Height = GridLength.Auto;
                barIsHidden = false;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            if (settings.PlayAreaSize != pendingPlayAreaSize && !isInGame) {
                settings.PlayAreaSize = pendingPlayAreaSize;
                needsReload = true;
            } else {
                UpdatePlayAreaSize();
            }

            if (settings.PlaySquareSize != pendingPlaySquareSize && !isInGame) {
                settings.PlaySquareSize = pendingPlaySquareSize;
                needsReload = true;
            } else {
                UpdatePlaySquareSize();
            }

            ShowHideBar();
            settingsBarIsLoaded = true;
        }

        private void FruitSpawnAmountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (settingsBarIsLoaded)
                UpdateFruitSpawnAmount();
        }

        private void UpdateFruitSpawnAmount() {
            settings.FruitSpawnAmount = (int)fruitSpawnAmountSlider.Value;
            fruitSpawnAmountLabel.Content = String.Format("Fruit Spawn No. : {0}", settings.FruitSpawnAmount);
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (settingsBarIsLoaded)
                UpdateSpeed();
        }

        private void UpdateSpeed() {
            timer.Interval = TimeSpan.FromMilliseconds((int)speedSlider.Value);
            speedLabel.Content = String.Format("Game Speed : {0}", (int)speedSlider.Value);
        }

        private void PlaySquareSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (settingsBarIsLoaded)
                UpdatePlaySquareSize();
        }

        private void UpdatePlaySquareSize() {
            pendingPlaySquareSize = (int)playSquareSizeSlider.Value;
            if (isInGame) {
                playSquareSizeSlider.Value = settings.PlaySquareSize;
                pendingPlaySquareSize = settings.PlaySquareSize;
            }
            playSquareSizeLabel.Content = String.Format("Square Size : {0}", pendingPlaySquareSize);
        }

        private void PlayAreaSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (settingsBarIsLoaded)
                UpdatePlayAreaSize();
        }

        private void UpdatePlayAreaSize() {
            pendingPlayAreaSize = (int)playAreaSizeSlider.Value;
            if (isInGame) {
                playAreaSizeSlider.Value = settings.PlayAreaSize;
                pendingPlayAreaSize = settings.PlayAreaSize;
            }
            playAreaSizeLabel.Content = String.Format("Grid Size : {0}", pendingPlayAreaSize);
        }

        private void CopyControl(Control sourceControl, Control targetControl) {
            if (sourceControl.GetType() != targetControl.GetType()) {
                throw new Exception("Incorrect control types");
            }

            foreach (PropertyInfo sourceProperty in sourceControl.GetType().GetProperties()) {
                object newValue = sourceProperty.GetValue(sourceControl, null);

                MethodInfo mi = sourceProperty.GetSetMethod(true);
                if (mi != null) {
                    sourceProperty.SetValue(targetControl, newValue, null);
                }
            }
        }

        private void Paint(List<List<Position>> positions, SolidColorBrush color1) {
            for (int i = 0; i < positions.Count; i++)
                for (int j = 0; j < positions[i].Count; j++) {
                    var currentSquare = GetSquare(positions[i][j]);

                    foreach (var label in currentSquare) {
                        var copy = new Label();
                        CopyControl(label, copy);
                        iconsOrig[String.Format("{0}_{1}", i.ToString(), j.ToString())] = copy;
                    }

                    switch (i) {
                        case (0):
                            break;
                        case (1):
                            SetSquareColor(positions[i][j], color1);
                            SetSquareContent(positions[i][j], null);
                            break;
                    }
                }
        }

        private void Wash(List<List<Position>> positions) {
            for (int i = 0; i < positions.Count; i++)
                for (int j = 0; j < positions[i].Count; j++) {
                    SetSquareColor(positions[i][j], (SolidColorBrush)iconsOrig[String.Format("{0}_{1}", i.ToString(), j.ToString())].Background);
                    ArrowFromDirection();
                }
            iconsOrig = new Dictionary<string, ContentControl>();
        }

        private void PauseIcon() {
            isPaused = true;
            timer.Stop();
            Paint(icons["pause"], new SolidColorBrush(Colors.DimGray));
        }

        private async void PlayIcon() {
            isCountingDown = true;
            Wash(icons["pause"]);
            for (int i = 3; i > 0; i--) {
                Paint(icons[i.ToString()], new SolidColorBrush(Colors.DimGray));
                await Task.Delay(1000);
                Wash(icons[i.ToString()]);
            }
            timer.Start();
            isPaused = false;
            isCountingDown = false;
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            if (isCountingDown) {
                return;
            } else if (isInGame && isPaused) {
                PlayIcon();
                return;
            } else if (isInGame && !isPaused) {
                PauseIcon();
                return;
            }

            foreach (Label playSquare in playArea.Children) {
                playSquare.Background = new SolidColorBrush(Colors.Black);
            }

            Reset();
            NewGame();
        }

        private void Reset() {
            timer.Stop();
            isInGame = false;

            playerPos = new List<Position>();
            fruitPos = new List<Position>();
            pendingDirection = 0;
            direction = 0;
            score = 0;

            Paint(icons["sad"], new SolidColorBrush(Colors.DimGray));
        }

        private void NewGame() {
            Paint(icons["sad"], new SolidColorBrush(Colors.Black));

            isInGame = true;

            playerPos.Add(RandomPosition(settings.PlayAreaSize / 2));
            SetSquareColor(playerPos[0], new SolidColorBrush(Colors.Red));

            PlaceFruit(null, true);
            
            timer.Start();
            UpdateScore();
        }

        private void TimerEvent(object sender, EventArgs e) {
            Move();
        }

        private void Move(int distance = 1) {
            List<Position> playerPosOrig = new List<Position>();

            foreach (Position position in playerPos) {
                SetSquareColor(position, new SolidColorBrush(Colors.Black));
                SetSquareContent(position, null);

                Position orig = new Position {
                    X = position.X,
                    Y = position.Y
                };
                playerPosOrig.Add(orig);
            }

            direction = pendingDirection;

            switch (direction) {
                case (0):
                    Position newPos0 = new Position {
                        X = playerPos[0].X,
                        Y = playerPos[0].Y - distance
                    };
                    if (newPos0.Y >= 0 && newPos0.Y < settings.PlayAreaSize) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos0.X && position.Y == newPos0.Y) {
                                Reset();
                                return;
                            }
                            position.Y -= distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (1):
                    Position newPos1 = new Position {
                        X = playerPos[0].X,
                        Y = playerPos[0].Y + distance
                    };
                    if (newPos1.Y >= 0 && newPos1.Y < settings.PlayAreaSize) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos1.X && position.Y == newPos1.Y) {
                                Reset();
                                return;
                            }
                            position.Y += distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (2):
                    Position newPos2 = new Position {
                        X = playerPos[0].X - distance,
                        Y = playerPos[0].Y
                    };
                    if (newPos2.X >= 0 && newPos2.X < settings.PlayAreaSize) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos2.X && position.Y == newPos2.Y) {
                                Reset();
                                return;
                            }
                            position.X -= distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (3):
                    Position newPos3 = new Position {
                        X = playerPos[0].X + distance,
                        Y = playerPos[0].Y
                    };
                    if (newPos3.X >= 0 && newPos3.X < settings.PlayAreaSize) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos3.X && position.Y == newPos3.Y) {
                                Reset();
                                return;
                            }
                            position.X += distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
            }

            PlaceFruit(playerPosOrig[0]);

            for (int i = 0; i < playerPos.Count; i++) {
                if (i != 0) {
                    playerPos[i] = playerPosOrig[i - 1];
                }
                SetSquareContent(playerPos[i], null);
                SetSquareColor(playerPos[i], new SolidColorBrush(Colors.Red));
            }

            ArrowFromDirection();
        }

        private void ArrowFromDirection() {
            switch (pendingDirection) {
                case (0):
                    SetSquareContent(playerPos[0], "▴", HorizontalAlignment.Center, VerticalAlignment.Top);
                    break;
                case (1):
                    SetSquareContent(playerPos[0], "▾", HorizontalAlignment.Center, VerticalAlignment.Bottom);
                    break;
                case (2):
                    SetSquareContent(playerPos[0], "◂", HorizontalAlignment.Left, VerticalAlignment.Center);
                    break;
                case (3):
                    SetSquareContent(playerPos[0], "▸", HorizontalAlignment.Right, VerticalAlignment.Center);
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (isInGame && !isPaused) {
                switch (e.Key) {
                    case (Key.Space):
                        playPauseButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        break;
                    case (Key.W):
                        if (direction != 1) {
                            pendingDirection = 0;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.S):
                        if (direction != 0) {
                            pendingDirection = 1;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.A):
                        if (direction != 3) {
                            pendingDirection = 2;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.D):
                        if (direction != 2) {
                            pendingDirection = 3;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Up):
                        if (direction != 1) {
                            pendingDirection = 0;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Down):
                        if (direction != 0) {
                            pendingDirection = 1;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Left):
                        if (direction != 3) {
                            pendingDirection = 2;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Right):
                        if (direction != 2) {
                            pendingDirection = 3;
                            ArrowFromDirection();
                        }
                        break;
                }
            }
        }

        private Position RandomPosition(int minDistanceFromEdge = 0) {
            Random rnd = new Random();
            Position position = new Position();
            bool isUnderSnake = false;

            do {
                position.X = rnd.Next(minDistanceFromEdge, settings.PlayAreaSize - minDistanceFromEdge);
                position.Y = rnd.Next(minDistanceFromEdge, settings.PlayAreaSize - minDistanceFromEdge);

                foreach (Position playerPos in playerPos) {
                    if (position.X == playerPos.X && position.Y == playerPos.Y) {
                        isUnderSnake = true;
                    } else {
                        isUnderSnake = false;
                    }
                }
            } while (isUnderSnake);

            return position;
        }

        private async void PlaceFruit(Position originalPosition = null, bool genNew = false) {
            List<Position> rm = new List<Position>();
            foreach (Position fruitPosition in fruitPos) {
                if (playerPos[0].X == fruitPosition.X && playerPos[0].Y == fruitPosition.Y) {
                    genNew = true;
                    rm.Add(fruitPosition);
                } else {
                    SetSquareColor(fruitPosition, new SolidColorBrush(Colors.Green));
                }
            }
            foreach (Position rmPos in rm) {
                fruitPos.Remove(rmPos);
            }
            if (genNew) {
                if (fruitPos.Count < 0) {
                    fruitSpawnAmountSlider.Minimum = 1;
                }

                for (int i = 0; i < settings.FruitSpawnAmount; i++) {
                    Position position = RandomPosition();
                    fruitPos.Add(position);
                    SetSquareColor(position, new SolidColorBrush(Colors.Green));
                    await Task.Delay(1);
                }
                
                if (originalPosition != null) {
                    playerPos.Add(AddBody(originalPosition));
                    UpdateScore(1);
                }

                if (fruitPos.Count <= 1) {
                    fruitSpawnAmountSlider.Minimum = 1;
                } else {
                    fruitSpawnAmountSlider.Minimum = 0;
                }
            }
        }

        private Position AddBody(Position originalPosition) {
            int xDif = playerPos[0].X - originalPosition.X;
            int yDif = playerPos[0].Y - originalPosition.Y;
            Position position = new Position {
                X = playerPos.Last().X - xDif,
                Y = playerPos.Last().Y - yDif
            };

            return position;
        }

        private void UpdateScore(int add = 0) {
            score += add;
            scoreLabel.Content = String.Format("Score : {0}", score);
            if (score >= highscore) {
                highscore = score;
                highScoreLabel.Content = String.Format("Highscore : {0}", highscore);
            }
        }
    }
}
