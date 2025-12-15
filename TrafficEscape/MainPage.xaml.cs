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
        double enemySpeed = 5;
        List<Image> coins = new List<Image>();
        int totalCoins = Preferences.Default.Get("TotalCoins", 0);
        IAudioPlayer gameMusic;
        IAudioPlayer gameOverSound;
        bool hasShield = false;
        

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

            await Task.WhenAll(
                PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear),
                ShieldIcon.TranslateTo(newX, ShieldIcon.TranslationY, 80, Easing.Linear)
                );
        }

        //MOVE RIGHT METHOD
        async void MoveRight(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX + MoveAmount;

            if (newX > rightBoundary)
                newX = rightBoundary;

            await Task.WhenAll(
                PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear),
                ShieldIcon.TranslateTo(newX, ShieldIcon.TranslationY, 80, Easing.Linear)
                );
        }

        //SPAWN ENEMY METHOD
        void SpawnEnemy()
        {
            if (!gameRunning)
                return;

            double roadWidth = PlayArea.Width;
            if (roadWidth <= 0)
                return;

            Image enemy = new Image
            {
                Source = enemyImages[rand.Next(enemyImages.Length)],
                WidthRequest = 80,
                HeightRequest = 120,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };

            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;

            int laneIndex = rand.Next(laneCount);

            double laneX = (laneWidth * laneIndex) + (laneWidth / 2) - (enemy.WidthRequest / 2);

            enemy.TranslationX = laneX;
            enemy.TranslationY = -200;

            if (PlayArea.Handler != null)
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

            List<Image> toRemove = new List<Image>();

            for (int i = 0; i < enemies.Count; i++)
            {
                Image e = enemies[i];

                if (!PlayArea.Children.Contains(e))
                {
                    toRemove.Add(e);
                    continue;
                }

                try
                {
                    // Move down
                    e.TranslationY += enemySpeed;
                }
                catch
                {
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

                if (CheckCollision(playerRect, enemyRect))
                {
                    if(hasShield)
                    {
                        hasShield = false;
                        ShieldIcon.IsVisible = false;
                        toRemove.Add(e);
                        continue;
                    }
                    else
                    {
                        EndGame();
                        return;
                    }
                }

                if (e.TranslationY > PlayArea.Height + 200)
                {
                    toRemove.Add(e);
                }
            }

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
            bool soundEnabled = Preferences.Get("SoundEnabled", true);

            if (soundEnabled)
            {
                gameMusic?.Play();
            }
            else
            {
                gameMusic?.Stop();
            }

            // Reset score
            score = 0;
            ScoreLabel.Text = "Score: 0";

            hasShield = false;
            ShieldIcon.IsVisible = false;

            gameRunning = true;

            StartEnemySpawner();

            // Difficulty timer
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (!gameRunning)
                {
                    return false;
                }

                enemySpeed += 0.6;
                if (enemySpeed > 22)
                {
                    enemySpeed = 22;
                }
                return true;
            });

            // Coin spawner
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1800), () =>
            {
                if (!gameRunning)
                {
                    return false;
                }
                SpawnCoin();
                return true;
            });

            // Shield spawner
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(4500), () =>
            {
                if (!gameRunning) return false;

                SpawnShield();
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

        //START ENEMY SPAWNER METHOD
        void StartEnemySpawner()
        {
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(700), () =>
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
                    if (coin.Source.ToString().Contains("shield"))
                    {
                        hasShield = true;
                        ShieldIcon.IsVisible = true;
                    }
                    else
                    {
                        totalCoins++;
                        Preferences.Default.Set("TotalCoins", totalCoins);
                    }

                    toRemove.Add(coin);
                    continue;
                }

                if (coin.TranslationY > PlayArea.Height + 200)
                {
                    toRemove.Add(coin);
                }
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

            bool soundEnabled = Preferences.Get("SoundEnabled", true);

            if (soundEnabled)
            {
                gameOverSound?.Play();
            }

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
            gameOverSound?.Stop();
            // Navigate back to Main Menu
            await Shell.Current.GoToAsync("..");
        }

        void SpawnCoin()
        {
            if (!gameRunning)
            {
                return;
            }

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

            if (PlayArea.Handler != null)
            {
                PlayArea.Children.Insert(1, coin);
                coins.Add(coin);
            }
        }

        void SpawnShield()
        {
            if (!gameRunning)
            {
                return;
            }

            if (hasShield)
            {
                return;
            }

            if (rand.NextDouble() > 0.45)
            {
                return;
            }

            double roadWidth = PlayArea.Width;
            if (roadWidth <= 0)
            {
                return;
            }

            Image shield = new Image
            {
                Source = "shield.png",
                WidthRequest = 60,
                HeightRequest = 60,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };

            int laneCount = 5;
            double laneWidth = roadWidth / laneCount;
            int laneIndex = rand.Next(laneCount);

            shield.TranslationX = (laneWidth * laneIndex) + (laneWidth / 2) - 30;
            shield.TranslationY = -200;

            PlayArea.Children.Insert(1, shield);
            coins.Add(shield);
        }
    }
}


