namespace TrafficEscape
{
    public partial class GamePage : ContentPage
    {
        List<Image> enemies = new List<Image>();
        Random rand = new Random();

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

        public GamePage()
        {
            InitializeComponent();

            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1200), () =>
            {
                SpawnEnemy();
                return true;
            });

            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                MoveEnemies();
                return true;
            });


            RoadView.Drawable = roadDrawable;

            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS

            timer.Tick += (s, e) =>
            {
                roadDrawable.Offset += roadDrawable.Speed;

                float height = (float)RoadView.Height;

                // Wrap offset so dash lines loop smoothly
                if (roadDrawable.Offset > height)
                {
                    roadDrawable.Offset = 0;
                }
                RoadView.Invalidate(); // redraw
            };

            timer.Start();
        }

        async void MoveLeft(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX - MoveAmount;

            if(newX < leftBoundary)
               newX = leftBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        async void MoveRight(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX + MoveAmount;

            if(newX > rightBoundary)
               newX = rightBoundary;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        void SpawnEnemy()
        {
            // Pick a random car
            string carImage = enemyImages[rand.Next(enemyImages.Length)];

            Image enemy = new Image
            {
                Source = carImage,
                WidthRequest = 80,
                HeightRequest = 120,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
            };

            PlayArea.Children.Add(enemy);
            enemies.Add(enemy);

            // === Dynamic lane calculations ===
            int laneCount = 5; // must match your road drawing (5 lanes)
            double roadWidth = PlayArea.Width;

            // If width is not measured yet, skip this spawn
            if (roadWidth <= 0)
                return;

            double laneWidth = roadWidth / laneCount;

            double[] lanes = new double[laneCount];

            for (int i = 0; i < laneCount; i++)
            {
                // Converts from X=0 at left to X=0 at center of play area
                lanes[i] = (-roadWidth / 2) + (laneWidth * i) + (laneWidth / 2);
            }

            // Pick lane
            double laneX = lanes[rand.Next(lanes.Length)];

            // Position the enemy
            enemy.TranslationX = laneX;
            enemy.TranslationY = -200;
        }

        void MoveEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Image e = enemies[i];

                // If the enemy has already been removed from the layout, skip it
                if (!PlayArea.Children.Contains(e))
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                // Move down
                e.TranslationY += 5;

                // Remove if off screen
                if (e.TranslationY > PlayArea.Height + 200)
                {
                    PlayArea.Children.Remove(e);
                    enemies.RemoveAt(i);
                }
            }
        }





    }
}
