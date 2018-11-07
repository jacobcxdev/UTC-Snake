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
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }




        /*    Classes    */

        
        class Utilities {

            // The Utilities class houses standalone "portable" functions which are used in the code later on.

            public void CopyControl(Control sourceControl, Control targetControl) {
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
        }

        class Position {

            // The Position class defines a class with which you can store coordinates for the playArea.

            public int X;
            public int Y;
        }

        class Settings {

            // The Settings class houses mutable settings which can be changed from the settings bar.

            public int PlayAreaSize;
            public int PlaySquareSize;
            public int FruitSpawnAmount;
        }

        class GameStateBools {

            // The GameStateBools class houses various bools which are used to log the game state.

            public bool settingsBarIsLoaded = false;
            public bool needsReload = false;
            public bool barIsHidden = true;
            public bool isInGame = false;
            public bool isPaused = false;
            public bool isCountingDown = false;
        }

        class GameState {

            // The GameState class houses values which are solely used by or set by the game.

            public int direction = 0;
            public int score = 0;
            public int highscore = 0;
        }

        class Pending {

            // The Pending class houses values which are used to store possible changes to various other values which are used directly by the game.

            public int pendingPlayAreaSize = 0;
            public int pendingPlaySquareSize = 0;
            public int pendingDirection = 0;
        }




        /*    Initialising Global Variables    */

        
        DispatcherTimer timer = new DispatcherTimer() {    // Timer is used for the snake's movement.
            Interval = TimeSpan.FromMilliseconds(200)
        };

        Utilities utilities = new Utilities();    // Utilities class houses utilities (see above - classes).

        Settings settings = new Settings() {    // Settings class houses game settings (see above - classes).
            PlayAreaSize = 25,
            PlaySquareSize = 20
        };

        GameStateBools gameStateBools = new GameStateBools();    // GameStateBools class houses bools used for logging the game state (see above - classes).
        GameState gameState = new GameState();    // GameState class houses values used by the game (see above - classes).
        Pending pending = new Pending();    // Pending class stores values for possible changes later on (see above - classes).

        Dictionary<string, List<List<Position>>> glyphs = new Dictionary<string, List<List<Position>>>();    // glyphs stores a list of positions for different colours for the glyphs.
        Dictionary<string, ContentControl> glyphsOrig = new Dictionary<string, ContentControl>();    // glyphsOrig stores the original label settings from before they were painted over.

        List<Position> snakePos = new List<Position>();    // snakePos is a list of positions which the snake consists of.
        List<Position> fruitPos = new List<Position>();    // fruitPos is a list of positions of various fruits.




        /*    Loading Functions    */

        
        private void Window_Loaded(object sender, RoutedEventArgs e) {    // Window_Loaded is executed when the window loads. It calls functions to construct the playArea and get everything ready for a new game to be started.
            timer.Tick += TimerEvent;
            LoadPlayArea();
            LoadGlyphs();
            UpdateScore();
            UpdatePlayAreaSize();
            UpdatePlaySquareSize();
            UpdateSpeed();
            UpdateFruitSpawnAmount();
            playPauseButton.Focus();
            ShowHideBar(true);
        }

        private void LoadPlayArea() {    // LoadPlayArea (clears and) constructs the playArea (grid).
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

        private void LoadGlyphs() {    // LoadGlyphs defines arrays which contain code for shapes and colours of glyphs. It then calls AddGlyph to add them to glyphs.
            int[][] one = new int[][] {
                new int[] { 0, 0, 1, 0, 0 },
                new int[] { 0, 1, 1, 0, 0 },
                new int[] { 1, 0, 1, 0, 0 },
                new int[] { 0, 0, 1, 0, 0 },
                new int[] { 1, 1, 1, 1, 1 }
            };

            int[][] two = new int[][] {
                new int[] { 0, 1, 1, 1, 0 },
                new int[] { 1, 0, 0, 0, 1 },
                new int[] { 0, 0, 1, 1, 0 },
                new int[] { 0, 1, 0, 0, 0 },
                new int[] { 1, 1, 1, 1, 1 }
            };

            int[][] three = new int[][] {
                new int[] { 0, 1, 1, 1, 0 },
                new int[] { 1, 0, 0, 0, 1 },
                new int[] { 0, 0, 1, 1, 0 },
                new int[] { 1, 0, 0, 0, 1 },
                new int[] { 0, 1, 1, 1, 0 }
            };

            int[][] pause = new int[][] {
                new int[] { 1, 1, 0, 1, 1 },
                new int[] { 1, 1, 0, 1, 1 },
                new int[] { 1, 1, 0, 1, 1 },
                new int[] { 1, 1, 0, 1, 1 },
                new int[] { 1, 1, 0, 1, 1 }
            };

            int[][] sad = new int[][] {
                new int[] { 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 1, 0, 1, 1, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 1 },
                new int[] { 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 1, 1, 1, 1, 0 },
                new int[] { 0, 1, 0, 0, 0, 1, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0 }
            };

            AddGlyph(one, "1");
            AddGlyph(two, "2");
            AddGlyph(three, "3");
            AddGlyph(pause, "pause");
            AddGlyph(sad, "sad");
        }

        private void AddGlyph(int[][] glyph, string name) {    // AddGlyph converts glyph arrays from LoadGlyphs into positions, and then adds them to glyphs.
            glyphs[name] = new List<List<Position>> { new List<Position>(), new List<Position>() };

            for (int i = 0; i < glyph.Length; i++)
                for (int j = 0; j < glyph.Length; j++) {
                    Position position = new Position {
                        X = settings.PlayAreaSize / 2 - glyph.Count() / 2 + j,
                        Y = settings.PlayAreaSize / 2 - glyph.Count() / 2 + i
                    };
                    switch (glyph[i][j]) {
                        case (0):
                            glyphs[name][0].Add(position);
                            break;
                        case (1):
                            glyphs[name][1].Add(position);
                            break;
                    }
                }
        }




        /*    User Input    */

        
        private void Window_KeyDown(object sender, KeyEventArgs e) {    // Window_KeyDown is called every time the user presses a key. If the key is one of W, A, S, D, Up, Down, Left, Right (movement direction) or Space (play/pause), it will call the relevant functions.
            if (gameStateBools.isInGame && !gameStateBools.isPaused) {
                switch (e.Key) {
                    case (Key.Space):    // Space "Clicks" the playPauseButton.
                        playPauseButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        break;
                    case (Key.W):    // W changes the pendingDirection to move up.
                        if (gameState.direction != 1) {
                            pending.pendingDirection = 0;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.S):    // S changes the pendingDirection to move down.
                        if (gameState.direction != 0) {
                            pending.pendingDirection = 1;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.A):    // A changes the pendingDirection to move left.
                        if (gameState.direction != 3) {
                            pending.pendingDirection = 2;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.D):    // D changes the pendingDirection to move right.
                        if (gameState.direction != 2) {
                            pending.pendingDirection = 3;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Up):    // Up changes the pendingDirection to move up.
                        if (gameState.direction != 1) {
                            pending.pendingDirection = 0;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Down):    // Down changes the pendingDirection to move down.
                        if (gameState.direction != 0) {
                            pending.pendingDirection = 1;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Left):    // Left changes the pendingDirection to move left.
                        if (gameState.direction != 3) {
                            pending.pendingDirection = 2;
                            ArrowFromDirection();
                        }
                        break;
                    case (Key.Right):    // Right changes the pendingDirection to move right.
                        if (gameState.direction != 2) {
                            pending.pendingDirection = 3;
                            ArrowFromDirection();
                        }
                        break;
                }
            }
        }




        /*    Moving    */

        
        private void TimerEvent(object sender, EventArgs e) {    // The TimerEvent is called every tick of the timer. TimerEvent then calls Move.
            Move();
        }

        private void Move(int distance = 1) {    // Move is the function which actually manages what happens each tick. This includes repositioning the snake and replacing fruit.
            List<Position> snakePosOrig = new List<Position>();    // snakePosOrig is a list of positions which stores the positions of the snake at the start of the Move function.
            gameState.direction = pending.pendingDirection;

            foreach (Position position in snakePos) {    // Sets snake colours to black and stores positions of the snake at the start of the Move function in snakePosOrig.
                SetSquareColor(position, new SolidColorBrush(Colors.Black));
                SetSquareContent(position, null);

                Position orig = new Position {
                    X = position.X,
                    Y = position.Y
                };
                snakePosOrig.Add(orig);
            }

            switch (gameState.direction) {    // Manages snake position movement and death logic.
                case (0):
                    if (snakePos[0].Y - distance >= 0 && snakePos[0].Y - distance < settings.PlayAreaSize) {
                        foreach (Position snakePosition in snakePos) {
                            if (snakePosition.X == snakePos[0].X && snakePosition.Y == snakePos[0].Y - distance) {
                                Reset();
                                return;
                            }
                            snakePosition.Y -= distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (1):
                    if (snakePos[0].Y + distance >= 0 && snakePos[0].Y + distance < settings.PlayAreaSize) {
                        foreach (Position snakePosition in snakePos) {
                            if (snakePosition.X == snakePos[0].X && snakePosition.Y == snakePos[0].Y + distance) {
                                Reset();
                                return;
                            }
                            snakePosition.Y += distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (2):
                    if (snakePos[0].X - distance >= 0 && snakePos[0].X - distance < settings.PlayAreaSize) {
                        foreach (Position snakePosition in snakePos) {
                            if (snakePosition.X == snakePos[0].X - distance && snakePosition.Y == snakePos[0].Y) {
                                Reset();
                                return;
                            }
                            snakePosition.X -= distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
                case (3):
                    if (snakePos[0].X + distance >= 0 && snakePos[0].X + distance < settings.PlayAreaSize) {
                        foreach (Position snakePosition in snakePos) {
                            if (snakePosition.X == snakePos[0].X + distance && snakePosition.Y == snakePos[0].Y) {
                                Reset();
                                return;
                            }
                            snakePosition.X += distance;
                        }
                    } else {
                        Reset();
                        return;
                    }
                    break;
            }

            PlaceFruit(snakePosOrig[0]);    // Replaces fruit. This includes generating new fruit and adding values to the score and growing the snake.

            for (int i = 0; i < snakePos.Count; i++) {    // Shifts and paints the snake positions.
                if (i != 0) {
                    snakePos[i] = snakePosOrig[i - 1];
                }
                SetSquareContent(snakePos[i], null);
                SetSquareColor(snakePos[i], new SolidColorBrush(Colors.Red));
            }

            ArrowFromDirection();    // Adds directional arrow to the snake head.
        }




        /*    Fruit    */

        
        private async void PlaceFruit(Position originalPosition = null, bool genNew = false) {    // PlaceFruit places fruit at their respective positions. It also generates new fruit if one has been eaten with a random position from RandomPosition, and dynamically adjusts the minimum value of the fruitSpawnAmountSlider as needed. It is async to prevent freezing with larger fruitSpawnAmounts.
            List<Position> rm = new List<Position>();

            foreach (Position fruitPosition in fruitPos) {
                if (snakePos[0].X == fruitPosition.X && snakePos[0].Y == fruitPosition.Y) {
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
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                }

                if (originalPosition != null && snakePos.Count != 0) {
                    snakePos.Add(AddBody(originalPosition));
                    UpdateScore(1);
                }

                if (fruitPos.Count <= 1) {
                    fruitSpawnAmountSlider.Minimum = 1;
                } else {
                    fruitSpawnAmountSlider.Minimum = 0;
                }
            }
        }

        private Position AddBody(Position originalPosition) {    // AddBody returns a position for a new part of the snake. It is called if the snake has eaten a fruit.
            int xDif = snakePos[0].X - originalPosition.X;
            int yDif = snakePos[0].Y - originalPosition.Y;
            Position position = new Position {
                X = snakePos.Last().X - xDif,
                Y = snakePos.Last().Y - yDif
            };

            if (position.X > settings.PlayAreaSize - 1 || position.X < 0) {
                position.X = snakePos.Last().X + xDif;
            }

            if (position.Y > settings.PlayAreaSize - 1 || position.Y < 0) {
                position.Y = snakePos.Last().Y + yDif;
            }

            return position;
        }

        private void UpdateScore(int add = 0) {    // UpdateScore is a function that updates the score after a new fruit has been eaten. It is also called at launch to set the scoreLabel and highScoreLabel contents.
            gameState.score += add;
            scoreLabel.Content = String.Format("Score : {0}", gameState.score);
            if (gameState.score >= gameState.highscore) {
                gameState.highscore = gameState.score;
                highScoreLabel.Content = String.Format("Highscore : {0}", gameState.highscore);
            }
        }




        /*    Play/Pause Button    */

        
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {    // PlayPauseButton_Click is called when the Play/Pause button is clicked. It determines whether to start a new game, pause a current game or continue a current game by checking some of the GameStateBools. It does each of these actions by calling another function. If it is to call Reset and NewGame, it will first reset the colour of the playSquares back to black.
            if (gameStateBools.isCountingDown) {
                return;
            } else if (gameStateBools.isInGame && gameStateBools.isPaused) {
                PlayGlyph();
                return;
            } else if (gameStateBools.isInGame && !gameStateBools.isPaused) {
                PauseGlyph();
                return;
            }

            foreach (Label playSquare in playArea.Children) {
                playSquare.Background = new SolidColorBrush(Colors.Black);
            }

            Reset();
            NewGame();
        }

        private void PauseGlyph() {    // PauseGlyph pauses the timer and paints the pause glyph.
            gameStateBools.isPaused = true;
            timer.Stop();
            Paint(glyphs["pause"], new SolidColorBrush(Colors.DimGray));
        }

        private async void PlayGlyph() {    // PlayGlyph plays the countdown and then starts the timer again.
            gameStateBools.isCountingDown = true;
            Wash(glyphs["pause"]);
            for (int i = 3; i > 0; i--) {
                Paint(glyphs[i.ToString()], new SolidColorBrush(Colors.DimGray));
                await Task.Delay(1000);
                Wash(glyphs[i.ToString()]);
            }
            timer.Start();
            gameStateBools.isPaused = false;
            gameStateBools.isCountingDown = false;
        }

        private void Reset() {    // Reset stops the timer and resets the snake positions and fruit positions, as well as the score and directions. It also then pains the "sad" glyph.
            timer.Stop();
            gameStateBools.isInGame = false;

            snakePos = new List<Position>();
            fruitPos = new List<Position>();
            pending.pendingDirection = 0;
            gameState.direction = 0;
            gameState.score = 0;
            fruitSpawnAmountSlider.Minimum = 1;

            Paint(glyphs["sad"], new SolidColorBrush(Colors.DimGray));
        }

        private void NewGame() {    // NewGame washes the "sad" glyph that was painted in Reset, and then sets the snake position to the centre of the playArea, places a fruit, starts the timer, and calls UpdateScore.
            Paint(glyphs["sad"], new SolidColorBrush(Colors.Black));

            gameStateBools.isInGame = true;

            snakePos.Add(RandomPosition(settings.PlayAreaSize / 2));
            SetSquareColor(snakePos[0], new SolidColorBrush(Colors.Red));

            PlaceFruit(null, true);

            timer.Start();
            UpdateScore();
        }




        /*    Settings Button    */

        
        private void SettingsButton_Click(object sender, RoutedEventArgs e) {    // SettingsButton_Click is called when the settings button is clicked. It toggles the settings bar's visibility and applies pending changes by calling ShowHideBar.
            if (settings.PlayAreaSize != pending.pendingPlayAreaSize && !gameStateBools.isInGame) {
                settings.PlayAreaSize = pending.pendingPlayAreaSize;
                gameStateBools.needsReload = true;
            } else {
                UpdatePlayAreaSize();
            }

            if (settings.PlaySquareSize != pending.pendingPlaySquareSize && !gameStateBools.isInGame) {
                settings.PlaySquareSize = pending.pendingPlaySquareSize;
                gameStateBools.needsReload = true;
            } else {
                UpdatePlaySquareSize();
            }

            ShowHideBar();
            gameStateBools.settingsBarIsLoaded = true;
        }

        private void ShowHideBar(bool hide = false) {    // ShowHideBar toggles the settings bar's visibility, as well as applying or resetting any pending changes (determined by some GameStateBools).
            if (hide || !gameStateBools.barIsHidden) {
                bar.RowDefinitions[1].Height = new GridLength(0);
                bar.RowDefinitions[2].Height = new GridLength(0);
                gameStateBools.barIsHidden = true;
                if (gameStateBools.needsReload) {
                    LoadPlayArea();
                    LoadGlyphs();
                }
                gameStateBools.needsReload = false;
            } else if (gameStateBools.barIsHidden) {
                bar.RowDefinitions[1].Height = GridLength.Auto;
                bar.RowDefinitions[2].Height = GridLength.Auto;
                gameStateBools.barIsHidden = false;
            }
        }




        /*    Settings Sliders    */

        
        private void PlayAreaSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {    // PlayAreaSizeSlider_ValueChanged is called when the value of the playAreaSizeSlider is changed. It calls UpdatePlayAreaSize.
            if (gameStateBools.settingsBarIsLoaded)
                UpdatePlayAreaSize();
        }

        private void PlaySquareSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {    // PlaySquareSizeSlider_ValueChanged is called when the value of the playSquareSizeSlider is changed. It calls UpdatePlaySquareSize.
            if (gameStateBools.settingsBarIsLoaded)
                UpdatePlaySquareSize();
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {    // SpeedSlider_ValueChanged is called when the value of the speedSlider is changed. It calls UpdateSpeed.
            if (gameStateBools.settingsBarIsLoaded)
                UpdateSpeed();
        }

        private void FruitSpawnAmountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {    // FruitSpawnAmountSlider_ValueChanged is called when the value of the fruitSpawnAmountSlider is changed. It calls UpdateFruitSpawnAmount.
            if (gameStateBools.settingsBarIsLoaded)
                UpdateFruitSpawnAmount();
        }

        private void UpdatePlayAreaSize() {    // UpdatePlayAreaSize sets the playAreaSize to the value of the playAreaSizeSlider, depending on gameStateBools.isInGame.
            pending.pendingPlayAreaSize = (int)playAreaSizeSlider.Value;
            if (gameStateBools.isInGame) {
                playAreaSizeSlider.Value = settings.PlayAreaSize;
                pending.pendingPlayAreaSize = settings.PlayAreaSize;
            }
            playAreaSizeLabel.Content = String.Format("Grid Size : {0}", pending.pendingPlayAreaSize);
        }

        private void UpdatePlaySquareSize() {    // UpdatePlaySquareSize sets the playSquareSize to the value of the playSquareSizeSlider, depending on gameStateBools.isInGame.
            pending.pendingPlaySquareSize = (int)playSquareSizeSlider.Value;
            if (gameStateBools.isInGame) {
                playSquareSizeSlider.Value = settings.PlaySquareSize;
                pending.pendingPlaySquareSize = settings.PlaySquareSize;
            }
            playSquareSizeLabel.Content = String.Format("Square Size : {0}", pending.pendingPlaySquareSize);
        }

        private void UpdateSpeed() {    // UpdateSpeed dynamically changes the speed of the timer at any time to the value of the speedSlider.
            timer.Interval = TimeSpan.FromMilliseconds((int)speedSlider.Value);
            speedLabel.Content = String.Format("Game Speed : {0}", (int)speedSlider.Value);
        }

        private void UpdateFruitSpawnAmount() {    // UpdateFruitSpawnAmount dynamically changes the fruit spawn amount at any time to the value of the fruitSpawnAmountSlider.
            settings.FruitSpawnAmount = (int)fruitSpawnAmountSlider.Value;
            fruitSpawnAmountLabel.Content = String.Format("Fruit Spawn No. : {0}", settings.FruitSpawnAmount);
        }




        /*    Utilities    */

        
        private void SetSquareColor(Position position, SolidColorBrush color) {    // SetSquareColor sets the colour for a label at a given position.
            ((Label)playArea.Children[position.X * settings.PlayAreaSize + position.Y]).Background = color;
        }

        private void SetSquareContent(Position position, object content, HorizontalAlignment hAlign = HorizontalAlignment.Center, VerticalAlignment vAlign = VerticalAlignment.Center) {    // SetSquareContent sets the content for a label at a given position with given alignments.
            ((Label)playArea.Children[position.X * settings.PlayAreaSize + position.Y]).HorizontalContentAlignment = hAlign;
            ((Label)playArea.Children[position.X * settings.PlayAreaSize + position.Y]).VerticalContentAlignment = vAlign;
            ((Label)playArea.Children[position.X * settings.PlayAreaSize + position.Y]).Content = content;
        }

        private void Paint(List<List<Position>> positions, SolidColorBrush color1) {    // Paint sets the colour for labels at given positions of a glyph to a given colour.
            for (int i = 0; i < positions.Count; i++)
                for (int j = 0; j < positions[i].Count; j++) {
                    var copy = new Label();
                    utilities.CopyControl(((Label)playArea.Children[positions[i][j].X * settings.PlayAreaSize + positions[i][j].Y]), copy);
                    glyphsOrig[String.Format("{0}_{1}", i.ToString(), j.ToString())] = copy;

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

        private void Wash(List<List<Position>> positions) {    // Wash resets the colours of the labels at given positions to how they were before they were painted.
            for (int i = 0; i < positions.Count; i++)
                for (int j = 0; j < positions[i].Count; j++) {
                    SetSquareColor(positions[i][j], (SolidColorBrush)glyphsOrig[String.Format("{0}_{1}", i.ToString(), j.ToString())].Background);
                    ArrowFromDirection();
                }
            glyphsOrig = new Dictionary<string, ContentControl>();
        }

        private void ArrowFromDirection() {    // ArrowFromDirection calls SetSquareContent to set the content of the snake head to a directional arrow pointing in the correct direction.
            switch (pending.pendingDirection) {
                case (0):
                    SetSquareContent(snakePos[0], "▴", HorizontalAlignment.Center, VerticalAlignment.Top);
                    break;
                case (1):
                    SetSquareContent(snakePos[0], "▾", HorizontalAlignment.Center, VerticalAlignment.Bottom);
                    break;
                case (2):
                    SetSquareContent(snakePos[0], "◂", HorizontalAlignment.Left, VerticalAlignment.Center);
                    break;
                case (3):
                    SetSquareContent(snakePos[0], "▸", HorizontalAlignment.Right, VerticalAlignment.Center);
                    break;
            }
        }

        private Position RandomPosition(int minDistanceFromEdge = 0) {    // RandomPosition returns a random position with a random X and Y coordinate for the playArea.
            Random rnd = new Random();
            Position position = new Position {
                X = rnd.Next(minDistanceFromEdge, settings.PlayAreaSize - minDistanceFromEdge),
                Y = rnd.Next(minDistanceFromEdge, settings.PlayAreaSize - minDistanceFromEdge)
            };

            return position;
        }
    }
}