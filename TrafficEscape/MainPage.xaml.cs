using Plugin.Maui.Audio;
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
        int lastLaneIndex = -1;
        double enemySpeed = 5;
        double[] laneLastY;
        List<Image> coins = new List<Image>();
        int totalCoins = Preferences.Default.Get("TotalCoins", 0); // saved coins
        double spawnInterval = 1200;
        IAudioPlayer gameMusic;
        IAudioPlayer gameOverSound;

        string[] enemyImages =
        {
            "black_car.png",
            "black_car.png",
            "blue_car.png",
            "blue_car.png",
            "green_car.png",
            "yellow_car.png",
            "truck.png"
        };

        // How far the car moves per click
        const double MoveAmount = 60;
        double leftBoundary = -490;
        double rightBoundary = 490;

        RoadDrawable roadDrawable = new RoadDrawable();

        async Task LoadGameAudio()
        {
            if (gameMusic == null)
            {
                var file = await FileSystem.OpenAppPackageFileAsync("GameAudio.mp3");
                gameMusic = AudioManager.Current.CreatePlayer(file);
                gameMusic.Loop = true;
            }

            if (gameOverSound == null)
            {
                var file2 = await FileSystem.OpenAppPackageFileAsync("GameOver.mp3");
                gameOverSound = AudioManager.Current.CreatePlayer(file2);
                gameOverSound.Loop = false;
            }
        }


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
                if (!gameRunning)
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

            if (newX < leftBoundary)
                newX = leftBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        //MOVE RIGHT METHOD
        async void MoveRight(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX + MoveAmount;

            if (newX > rightBoundary)
                newX = rightBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        //SPAWN ENEMY METHOD
        void SpawnEnemy()
        {
            if (!gameRunning)
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
            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;

            double[] lanes = new double[laneCount];

            for (int i = 0; i < laneCount; i++)
            {
                lanes[i] = (laneWidth * i) + (laneWidth / 2) - (enemy.WidthRequest / 2);
            }

            int laneIndex;
            do
            {
                laneIndex = rand.Next(laneCount);
            } while (laneIndex == lastLaneIndex);

            lastLaneIndex = laneIndex;

            // Dynamic gap increases as enemies go faster
            double minGap = 100 + (enemySpeed * 2);

            // Prevent spawning too close vertically
            if (laneLastY[laneIndex] > -minGap)
            {
                return;
            }



            // Set enemy position BEFORE adding to layout
            enemy.TranslationX = lanes[laneIndex];
            enemy.TranslationY = -200;

            laneLastY[laneIndex] = -200;

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
                    e.TranslationY += enemySpeed; // Move down
                    int laneIndex = GetLaneIndex(e.TranslationX);
                    laneLastY[laneIndex] = e.TranslationY;
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
                    int laneIndex = GetLaneIndex(e.TranslationX);
                    laneLastY[laneIndex] = -500;
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
        async Task StartGame()
        {

            await LoadGameAudio();
            gameMusic?.Play();

            // Reset score
            score = 0;
            ScoreLabel.Text = "Score: 0";

            // Start game
            gameRunning = true;

            // Enemy spawner — we recreate it when spawnInterval changes
            StartEnemySpawner();

            // Difficulty timer (every 5 seconds)
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (!gameRunning)
                    return false;

                // Increase enemy speed
                enemySpeed += 0.8;
                if (enemySpeed > 25)
                    enemySpeed = 25;

                // Make enemies spawn a little faster
                spawnInterval -= 100;
                if (spawnInterval < 500)
                    spawnInterval = 500;

                // Restart spawner with new interval
                StartEnemySpawner();

                return true;
            });

            // Coin spawner
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1800), () =>
            {
                if (!gameRunning) return false;
                SpawnCoin();
                return true;
            });

            // Movement update
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                if (!gameRunning) return false;

                MoveEnemies();
                MoveCoins();
                return true;
            });

            // Score timer
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!gameRunning) return false;

                score += 5;
                ScoreLabel.Text = $"Score: {score}";
                return true;
            });
        }

        //START ENEMY TIMER METHOD
        void StartEnemySpawner()
        {
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(spawnInterval), () =>
            {
                if (!gameRunning)
                    return false;

                SpawnEnemy();
                return true;
            });
        }



        //MOVE COINS METHOD
        void MoveCoins()
        {
            if (!gameRunning)
                return;

            List<Image> toRemove = new List<Image>();

            foreach (var coin in coins)
            {
                // Move coin down
                coin.TranslationY += enemySpeed;

                // Collision rectangle with padding
                Rect playerRect = new Rect(
                    PlayerCar.X + PlayerCar.TranslationX + 20,
                    PlayerCar.Y + PlayerCar.TranslationY + 20,
                    PlayerCar.Width - 40,
                    PlayerCar.Height - 40);

                Rect coinRect = new Rect(
                    coin.X + coin.TranslationX,
                    coin.Y + coin.TranslationY,
                    coin.Width,
                    coin.Height);

                // Collect coin
                if (playerRect.IntersectsWith(coinRect))
                {
                    totalCoins++;
                    Preferences.Default.Set("TotalCoins", totalCoins);
                    toRemove.Add(coin);
                    continue;
                }

                // Off screen → remove
                if (coin.TranslationY > PlayArea.Height + 200)
                    toRemove.Add(coin);
            }

            // Remove safely
            foreach (var coin in toRemove)
            {
                PlayArea.Children.Remove(coin);
                coins.Remove(coin);
            }
        }



        async void PlayArea_SizeChanged(object sender, EventArgs e)
        {
            if (gameRunning)
                return;

            if (PlayArea.Width > 0 && PlayArea.Height > 0)
            {
                laneLastY = new double[5];
                for (int i = 0; i < 5; i++)
                {
                    laneLastY[i] = -500;
                }

                gameRunning = true;
                await StartGame();
            }
        }
        bool CheckCollision(Rect player, Rect enemy)
        {
            return player.IntersectsWith(enemy);
        }

        //ENDGAME METHOD
        async Task EndGame()
        {
            gameMusic?.Stop();
            gameOverSound?.Play();

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

        int GetLaneIndex(double x)
        {
            double roadWidth = PlayArea.Width;
            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;

            int index = (int)((x + roadWidth / 2) / laneWidth);
            return Math.Clamp(index, 0, laneCount - 1);
        }

        void SpawnCoin()
        {
            if (!gameRunning)
                return;

            double roadWidth = PlayArea.Width;
            if (roadWidth <= 0)
                return;

            // Create coin image
            Image coin = new Image
            {
                Source = "coin.png",
                WidthRequest = 50,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };

            // Lanes (same as enemies)
            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;

            double[] lanes = new double[laneCount];
            for (int i = 0; i < laneCount; i++)
                lanes[i] = (laneWidth * i) + (laneWidth / 2) - (coin.WidthRequest / 2);

            int laneIndex = rand.Next(laneCount);
            double laneX = lanes[laneIndex];

            // Position coin
            coin.TranslationX = laneX;
            coin.TranslationY = -200;

            // Add to play area
            if (PlayArea.Handler != null)
            {
                // Coins MUST be behind cars → add BEFORE enemies!
                PlayArea.Children.Insert(1, coin);
                coins.Add(coin);
            }
        }
    }
}


