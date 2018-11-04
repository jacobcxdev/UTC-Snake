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
        List<Position> playerPos = new List<Position> { };
        Position fruitPos = new Position();
        DispatcherTimer timer = new DispatcherTimer();


        bool isInGame = false;
        bool isPaused = false;
        int pendingDirection = 0;
        int direction = 0;
        int score = 0;
        int highscore = 0;


        private void setSquareColor(Position position, SolidColorBrush color) {
            var currentSquare = playArea.Children
                                 .OfType<ContentControl>()
                                 .Where(z => z.Name.StartsWith(String.Format("playSquare_{0}_{1}_", position.X, position.Y)));

            foreach (var label in currentSquare) {
                label.Background = color;
            }
        }

        private void setSquareContent(Position position, string content, HorizontalAlignment hAlign = HorizontalAlignment.Center, VerticalAlignment vAlign = VerticalAlignment.Center) {
            var currentSquare = playArea.Children
                                 .OfType<ContentControl>()
                                 .Where(z => z.Name.StartsWith(String.Format("playSquare_{0}_{1}_", position.X, position.Y)));

            foreach (var playSquare in currentSquare) {
                playSquare.HorizontalContentAlignment = hAlign;
                playSquare.VerticalContentAlignment = vAlign;

                playSquare.Content = content;
            }
        }

        private void loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i < geo.MaxX; i++) {
                for (int j = 0; j < geo.MaxY; j++) {
                    ColumnDefinition column = new ColumnDefinition();
                    column.Width = GridLength.Auto;
                    RowDefinition row = new RowDefinition();
                    row.Height = GridLength.Auto;

                    Label playSquare = new Label();
                    playSquare.Width = geo.Width;
                    playSquare.Height = geo.Width;
                    playSquare.Background = new SolidColorBrush(Colors.Black);
                    playSquare.Name = String.Format("playSquare_{0}_{1}_", i, j);
                    playSquare.Padding = new Thickness(0);
                    playSquare.HorizontalContentAlignment = HorizontalAlignment.Center;
                    playSquare.VerticalContentAlignment = VerticalAlignment.Center;
                    
                    playArea.ColumnDefinitions.Add(column);
                    playArea.RowDefinitions.Add(row);
                    playArea.Children.Add(playSquare);

                    Grid.SetColumn(playSquare, i);
                    Grid.SetRow(playSquare, j);
                }
            }

            updateScore();

            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += timerEvent;
        }

        private void playPause_Click(object sender, RoutedEventArgs e) {
            if (isInGame && isPaused) {
                timer.Start();
                isPaused = false;
                return;
            } else if (isInGame && !isPaused) {
                timer.Stop();
                isPaused = true;
                return;
            }

            foreach (Label playSquare in playArea.Children) {
                playSquare.Background = new SolidColorBrush(Colors.Black);
            }

            reset();
            newGame();
        }

        private void reset() {
            timer.Stop();
            isInGame = false;

            playerPos = new List<Position> { };
            pendingDirection = 0;
            direction = 0;
            score = 0;
        }

        private void newGame() {
            isInGame = true;

            playerPos.Add(randomPosition(geo.MaxX / 2));
            setSquareColor(playerPos[0], new SolidColorBrush(Colors.Red));

            placeFruit(true);
            
            timer.Start();
            updateScore();
        }

        private void timerEvent(object sender, EventArgs e) {
            move();
        }

        private void move(int distance = 1) {
            List<Position> playerPosOrig = new List<Position> { };

            foreach (Position position in playerPos) {
                setSquareColor(position, new SolidColorBrush(Colors.Black));

                Position orig = new Position();
                orig.X = position.X;
                orig.Y = position.Y;
                playerPosOrig.Add(orig);
            }

            direction = pendingDirection;

            switch (direction) {
                case (0):
                    Position newPos0 = new Position();
                    newPos0.X = playerPos[0].X;
                    newPos0.Y = playerPos[0].Y - distance;
                    if (newPos0.Y >= 0 && newPos0.Y < geo.MaxX) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos0.X && position.Y == newPos0.Y) {
                                reset();
                                return;
                            }
                            position.Y -= distance;
                        }
                    } else {
                        reset();
                        return;
                    }
                    break;
                case (1):
                    Position newPos1 = new Position();
                    newPos1.X = playerPos[0].X;
                    newPos1.Y = playerPos[0].Y + distance;
                    if (newPos1.Y >= 0 && newPos1.Y < geo.MaxX) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos1.X && position.Y == newPos1.Y) {
                                reset();
                                return;
                            }
                            position.Y += distance;
                        }
                    } else {
                        reset();
                        return;
                    }
                    break;
                case (2):
                    Position newPos2 = new Position();
                    newPos2.X = playerPos[0].X - distance;
                    newPos2.Y = playerPos[0].Y;
                    if (newPos2.X >= 0 && newPos2.X < geo.MaxX) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos2.X && position.Y == newPos2.Y) {
                                reset();
                                return;
                            }
                            position.X -= distance;
                        }
                    } else {
                        reset();
                        return;
                    }
                    break;
                case (3):
                    Position newPos3 = new Position();
                    newPos3.X = playerPos[0].X + distance;
                    newPos3.Y = playerPos[0].Y;
                    if (newPos3.X >= 0 && newPos3.X < geo.MaxX) {
                        foreach (Position position in playerPos) {
                            if (position.X == newPos3.X && position.Y == newPos3.Y) {
                                reset();
                                return;
                            }
                            position.X += distance;
                        }
                    } else {
                        reset();
                        return;
                    }
                    break;
            }

            if (fruitPos.X == playerPos[0].X && fruitPos.Y == playerPos[0].Y) {
                addBody(playerPosOrig[0]);
                updateScore(1);
            }

            for (int i = 0; i < playerPos.Count; i++) {
                if (i != 0) {
                    playerPos[i] = playerPosOrig[i - 1];
                }
                setSquareContent(playerPos[i], null);
                setSquareColor(playerPos[i], new SolidColorBrush(Colors.Red));
            }


            arrowFromDirection();

            placeFruit();
        }

        private void arrowFromDirection() {
            switch (pendingDirection) {
                case (0):
                    setSquareContent(playerPos[0], "▴", HorizontalAlignment.Center, VerticalAlignment.Top);
                    break;
                case (1):
                    setSquareContent(playerPos[0], "▾", HorizontalAlignment.Center, VerticalAlignment.Bottom);
                    break;
                case (2):
                    setSquareContent(playerPos[0], "◂", HorizontalAlignment.Left, VerticalAlignment.Center);
                    break;
                case (3):
                    setSquareContent(playerPos[0], "▸", HorizontalAlignment.Right, VerticalAlignment.Center);
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (isInGame) {
                switch (e.Key) {
                    case (Key.W):
                        if (direction != 1) {
                            pendingDirection = 0;
                            arrowFromDirection();
                        }
                        break;
                    case (Key.S):
                        if (direction != 0) {
                            pendingDirection = 1;
                            arrowFromDirection();
                        }
                        break;
                    case (Key.A):
                        if (direction != 3) {
                            pendingDirection = 2;
                            arrowFromDirection();
                        }
                        break;
                    case (Key.D):
                        if (direction != 2) {
                            pendingDirection = 3;
                            arrowFromDirection();
                        }
                        break;
                }
            }
        }

        private Position randomPosition(int minDistanceFromEdge = 0) {
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

        private void placeFruit(bool genNew = false) {
            if (genNew) {
                fruitPos = randomPosition();
            }
            while (fruitPos.X == playerPos[0].X && fruitPos.Y == playerPos[0].Y) {
                fruitPos = randomPosition();
            }
            setSquareColor(fruitPos, new SolidColorBrush(Colors.Green));
        }

        private void addBody(Position originalPosition) {
            int xDif = playerPos[0].X - originalPosition.X;
            int yDif = playerPos[0].Y - originalPosition.Y;
            Position position = new Position();

            position.X = playerPos.Last().X - xDif;
            position.Y = playerPos.Last().Y - yDif;

            playerPos.Add(position);
        }

        private void updateScore(int add = 0) {
            score += add;
            scoreLabel.Content = String.Format("Score : {0}", score);
            if (score >= highscore) {
                highscore = score;
                highScoreLabel.Content = String.Format("Highscore : {0}", highscore);
            }
        }
    }
}
