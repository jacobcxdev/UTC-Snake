using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        class Geometry {
            public int MaxX = 25;
            public int MaxY = 25;
            public int Width = 20;
            public int Height = 20;
        }

        class Position {
            public int X;
            public int Y;
        }

        Geometry geo = new Geometry();

        List<Position> playerPos = new List<Position>();
        List<Position> fruitPos = new List<Position>();

        Dictionary<string, List<List<Position>>> icons = new Dictionary<string, List<List<Position>>> {
            { "1", new List<List<Position>> { new List<Position>(), new List<Position>() } },
            { "2", new List<List<Position>> { new List<Position>(), new List<Position>() } },
            { "3", new List<List<Position>> { new List<Position>(), new List<Position>() } },
            { "pause", new List<List<Position>> { new List<Position>(), new List<Position>() } }
        };
        Dictionary<string, ContentControl> iconsOrig = new Dictionary<string, ContentControl>();

        DispatcherTimer timer = new DispatcherTimer() {
            Interval = TimeSpan.FromMilliseconds(200)
        };


        bool isInGame = false;
        bool isPaused = false;
        bool isCountingDown = false;
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
            for (int i = 0; i < geo.MaxX; i++) {
                for (int j = 0; j < geo.MaxY; j++) {
                    ColumnDefinition column = new ColumnDefinition {
                        Width = GridLength.Auto
                    };
                    RowDefinition row = new RowDefinition {
                        Height = GridLength.Auto
                    };

                    Label playSquare = new Label {
                        Width = geo.Width,
                        Height = geo.Width,
                        Background = new SolidColorBrush(Colors.Black),
                        Name = String.Format("playSquare_{0}_{1}_", i, j),
                        Padding = new Thickness(0),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    playArea.ColumnDefinitions.Add(column);
                    playArea.RowDefinitions.Add(row);
                    playArea.Children.Add(playSquare);

                    Grid.SetColumn(playSquare, i);
                    Grid.SetRow(playSquare, j);
                }
            }

            Dictionary<int, int[]> one = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 0, 1, 0, 0 } },
                { 1, new int[] { 0, 1, 1, 0, 0 } },
                { 2, new int[] { 1, 0, 1, 0, 0 } },
                { 3, new int[] { 0, 0, 1, 0, 0 } },
                { 4, new int[] { 1, 1, 1, 1, 1 } },
            };

            for (int i = 0; i < one.Count; i++)
                for (int j = 0; j < one.Count; j++) {
                    Position position = new Position {
                        X = geo.MaxX / 2 - one.Count / 2 + j,
                        Y = geo.MaxY / 2 - one.Count / 2 + i
                    };
                    switch (one[i][j]) {
                        case (0):
                            icons["1"][0].Add(position);
                            break;
                        case (1):
                            icons["1"][1].Add(position);
                            break;
                    }
                }

            Dictionary<int, int[]> two = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 1, 1, 1, 0 } },
                { 1, new int[] { 1, 0, 0, 0, 1 } },
                { 2, new int[] { 0, 0, 1, 1, 0 } },
                { 3, new int[] { 0, 1, 0, 0, 0 } },
                { 4, new int[] { 1, 1, 1, 1, 1 } },
            };

            for (int i = 0; i < two.Count; i++)
                for (int j = 0; j < two.Count; j++) {
                    Position position = new Position {
                        X = geo.MaxX / 2 - two.Count / 2 + j,
                        Y = geo.MaxY / 2 - two.Count / 2 + i
                    };
                    switch (two[i][j]) {
                        case (0):
                            icons["2"][0].Add(position);
                            break;
                        case (1):
                            icons["2"][1].Add(position);
                            break;
                    }
                }

            Dictionary<int, int[]> three = new Dictionary<int, int[]> {
                { 0, new int[] { 0, 1, 1, 1, 0 } },
                { 1, new int[] { 1, 0, 0, 0, 1 } },
                { 2, new int[] { 0, 0, 1, 1, 0 } },
                { 3, new int[] { 1, 0, 0, 0, 1 } },
                { 4, new int[] { 0, 1, 1, 1, 0 } },
            };

            for (int i = 0; i < three.Count; i++)
                for (int j = 0; j < three.Count; j++) {
                    Position position = new Position {
                        X = geo.MaxX / 2 - three.Count / 2 + j,
                        Y = geo.MaxY / 2 - three.Count / 2 + i
                    };
                    switch (three[i][j]) {
                        case (0):
                            icons["3"][0].Add(position);
                            break;
                        case (1):
                            icons["3"][1].Add(position);
                            break;
                    }
                }

            Dictionary<int, int[]> pause = new Dictionary<int, int[]> {
                { 0, new int[] { 1, 1, 0, 1, 1 } },
                { 1, new int[] { 1, 1, 0, 1, 1 } },
                { 2, new int[] { 1, 1, 0, 1, 1 } },
                { 3, new int[] { 1, 1, 0, 1, 1 } },
                { 4, new int[] { 1, 1, 0, 1, 1 } },
            };

            for (int i = 0; i < pause.Count; i++)
                for (int j = 0; j < pause.Count; j++) {
                    Position position = new Position {
                        X = geo.MaxX / 2 - pause.Count / 2 + j,
                        Y = geo.MaxY / 2 - pause.Count / 2 + i
                    };
                    switch (pause[i][j]) {
                        case (0):
                            icons["pause"][0].Add(position);
                            break;
                        case (1):
                            icons["pause"][1].Add(position);
                            break;
                    }
                }

            timer.Tick += TimerEvent;

            UpdateScore();

            play.Focus();
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
            Paint(icons["pause"], new SolidColorBrush(Colors.SlateGray));
        }

        private async void PlayIcon() {
            isCountingDown = true;
            Wash(icons["pause"]);
            for (int i = 3; i > 0; i--) {
                Paint(icons[i.ToString()], new SolidColorBrush(Colors.SlateGray));
                await Task.Delay(1000);
                Wash(icons[i.ToString()]);
            }
            timer.Start();
            isPaused = false;
            isCountingDown = false;
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e) {
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
        }

        private void NewGame() {
            isInGame = true;

            playerPos.Add(RandomPosition(geo.MaxX / 2));
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
                    if (newPos0.Y >= 0 && newPos0.Y < geo.MaxX) {
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
                    if (newPos1.Y >= 0 && newPos1.Y < geo.MaxX) {
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
                    if (newPos2.X >= 0 && newPos2.X < geo.MaxX) {
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
                    if (newPos3.X >= 0 && newPos3.X < geo.MaxX) {
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
                position.X = rnd.Next(minDistanceFromEdge, geo.MaxX - minDistanceFromEdge);
                position.Y = rnd.Next(minDistanceFromEdge, geo.MaxY - minDistanceFromEdge);

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

        private void PlaceFruit(Position originalPosition = null, bool genNew = false) {
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
                Position position = RandomPosition();
                fruitPos.Add(position);
                SetSquareColor(position, new SolidColorBrush(Colors.Green));
                
                if (originalPosition != null) {
                    playerPos.Add(AddBody(originalPosition));
                    UpdateScore(1);
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
