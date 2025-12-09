using System.Threading.Tasks;

namespace TrafficEscape
{
    public partial class GamePage : ContentPage
    {
        List<Image> enemies = new List<Image>();
        Random rand = new Random();
        double collisionPadding = 20;
        bool gameRunning = false;
        int score = 0;
        int highScore = 0;

        string[] enemyImages =
        {
            "black_car.png",
            "blue_car.png",
            "green_car.png",
            "yellow_car.png",
            "truck.png"
        };

        // How far the car moves per click
        const double MoveAmount = 30;
        double leftBoundary = -490;
        double rightBoundary = 490;

        RoadDrawable roadDrawable = new RoadDrawable();

        //CONSTRUCTOR
        public GamePage()
        {
            InitializeComponent();

            highScore = Preferences.Get("HighScore", 0);

            RoadView.Drawable = roadDrawable;

            PlayArea.SizeChanged += PlayArea_SizeChanged;

            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS

            timer.Tick += (s, e) =>
            {
                if(!gameRunning)
                {
                    return;
                }

                roadDrawable.Offset += roadDrawable.Speed;

                float height = (float)RoadView.Height;

                // Wraps around so lines loop smoothly
                if (roadDrawable.Offset > height)
                {
                    roadDrawable.Offset = 0;
                }
                RoadView.Invalidate();
            };

            timer.Start();
        }

        //MOVE LEFT METHOD
        async void MoveLeft(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX - MoveAmount;

            if(newX < leftBoundary)
               newX = leftBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        //MOVE RIGHT METHOD
        async void MoveRight(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX + MoveAmount;

            if(newX > rightBoundary)
               newX = rightBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        //SPAWN ENEMY METHOD
        void SpawnEnemy()
        {
            if(!gameRunning)
            {
                return;
            }

            double roadWidth = PlayArea.Width;

            // If width not ready, try again later
            if (roadWidth <= 0)
                return;

            //random enemy
            string carImage = enemyImages[rand.Next(enemyImages.Length)];

            //Create enemy
            Image enemy = new Image
            {
                Source = carImage,
                WidthRequest = 80,
                HeightRequest = 120,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
            };

            // Calculate lanes
            // Calculate lanes (absolute positions)
            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;

            double[] lanes = new double[laneCount];

            for (int i = 0; i < laneCount; i++)
            {
                lanes[i] = (laneWidth * i) + (laneWidth / 2) - (enemy.WidthRequest / 2);
            }


            // Pick lane
            double laneX = lanes[rand.Next(lanes.Length)];

            // Set enemy position BEFORE adding to layout
            enemy.TranslationX = laneX;
            enemy.TranslationY = -200;

            // Now add safely
            if (PlayArea.Handler != null) // ensures UI is ready
            {
                PlayArea.Children.Add(enemy);
                enemies.Add(enemy);
            }

        }

        //MOVE ENEMY METHOD
        void MoveEnemies()
        {
            if (!gameRunning)
            {
                return;
            }
              
            // A list of enemies that should be removed at the end
            List<Image> toRemove = new List<Image>();

            // First pass: move & mark
            for (int i = 0; i < enemies.Count; i++)
            {
                Image e = enemies[i];

                // If it no longer exists in the layout, mark it
                if (!PlayArea.Children.Contains(e))
                {
                    toRemove.Add(e);
                    continue;
                }

                try
                {
                    e.TranslationY += 5; // Move down
                }
                catch
                {
                    // If MAUI disposed it mid-frame, mark for removal
                    toRemove.Add(e);
                    continue;
                }

                // Build rectangles for collision
                Rect playerRect = new Rect(
                    PlayerCar.X + PlayerCar.TranslationX + collisionPadding,
                    PlayerCar.Y + PlayerCar.TranslationY + collisionPadding,
                    PlayerCar.Width - (collisionPadding * 2),
                    PlayerCar.Height - (collisionPadding * 2));

                Rect enemyRect = new Rect(
                    e.X + e.TranslationX + collisionPadding,
                    e.Y + e.TranslationY + collisionPadding,
                    e.Width - (collisionPadding * 2),
                    e.Height - (collisionPadding * 2));



                // Check collision
                if (CheckCollision(playerRect, enemyRect))
                {
                    EndGame();
                    return;
                }


                // If off the screen, mark it
                if (e.TranslationY > PlayArea.Height + 200)
                {
                    toRemove.Add(e);
                }
            }

            // Second pass: safely remove all marked enemies
            foreach (var enemy in toRemove)
            {
                if (PlayArea.Children.Contains(enemy))
                {
                    try { PlayArea.Children.Remove(enemy); }
                    catch { }
                }

                enemies.Remove(enemy);
            }
        }

        //STARTGAME METHOD
        void StartGame()
        {
            // Start with a fresh score
            score = 0;
            ScoreLabel.Text = "Score: 0";

            // Turn the game on
            gameRunning = true;

            // Enemy spawner timer
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1200), () =>
            {
                if (!gameRunning)
                    return false;

                SpawnEnemy();
                return true;
            });

            // Enemy movement timer
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                if (!gameRunning)
                    return false;

                MoveEnemies();
                return true;
            });

            // Score timer
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!gameRunning)
                    return false;

                score += 5;
                ScoreLabel.Text = "Score: " + score;

                return true;
            });
        }


        void PlayArea_SizeChanged(object sender, EventArgs e)
        {
            if (gameRunning)
                return;

            if (PlayArea.Width > 0 && PlayArea.Height > 0)
            {
                StartGame();
            }
        }
        bool CheckCollision(Rect player, Rect enemy)
        {
            return player.IntersectsWith(enemy);
        }

        //ENDGAME METHOD
        async Task EndGame()
        {
            // Stop the game
            gameRunning = false;

            // Load the saved high score
            int highScore = Preferences.Default.Get("HighScore", 0);

            // If the player beat the high score, save it
            if (score > highScore)
            {
                highScore = score;
                Preferences.Default.Set("HighScore", highScore);
            }

            // Update Game Over screen text
            GameOverScore.Text = $"Score: {score}";
            GameOverHighScore.Text = $"High Score: {highScore}";

            // Show the Game Over overlay
            GameOverOverlay.IsVisible = true;
            GameOverOverlay.Opacity = 0;
            await GameOverOverlay.FadeTo(1, 800, Easing.CubicIn);
        }

        async void ReturnToMenu_Clicked(object sender, EventArgs e)
        {
            // Navigate back to Main Menu
            await Shell.Current.GoToAsync("..");
        }


    }
}
