using System.Timers;

namespace TrafficEscape
{
    public partial class MainPage : ContentPage
    {
        /*
         60 fps, 
        update() which will do all the math
        redraw()
         
        */

        Image carImage;
        Image roadImage1;
        Image roadImage2;
        Image roadImage3;
        int roadHeight = 800;


        AbsoluteLayout gameLayout;


        System.Timers.Timer gameTimer;

        public void setUpGameLayout()
        {
            gameLayout = new AbsoluteLayout
            {
                WidthRequest = 400,
                HeightRequest = 800,
                
            };
        }

        public void setUpPlayerImage() {    


             carImage = new Image
            {
                Source = "player_red_car.png",
                WidthRequest = 100,
                HeightRequest = 200,
            };

            roadImage1 = new Image
            {
                Source = "road.png",
                WidthRequest = 400,
                HeightRequest = roadHeight,
            };

            roadImage2 = new Image
            {
                Source = "road.png",
                WidthRequest = 400,
                HeightRequest = roadHeight,
            };

            roadImage3 = new Image
            {
                Source = "road.png",
                WidthRequest = 400,
                HeightRequest = roadHeight,
            };

            AbsoluteLayout.SetLayoutBounds(roadImage1, new Rect(gameLayout.Width / 2 - roadImage1.Width / 2, 0, 400, 1000));
            AbsoluteLayout.SetLayoutBounds(roadImage2, new Rect(gameLayout.Width / 2 - roadImage2.Width / 2, 0, 400, 200));
            AbsoluteLayout.SetLayoutBounds(roadImage3, new Rect(gameLayout.Width / 2 - roadImage3.Width / 2, 0, 400, -150));
            AbsoluteLayout.SetLayoutBounds(carImage, new Rect(100,100 ,400, 400));


        }

        public MainPage()
        {
            InitializeComponent();
            setUpGameLayout();
            Content = gameLayout;
            setUpTimer();
        }

        public void setUpTimer()
        {
            gameTimer = new System.Timers.Timer(16);
            gameTimer.Elapsed += gameTimerElapsed;
            gameTimer.Start();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            setUpPlayerImage();

            setUpPlayerImage();
            gameLayout.Children.Add(roadImage1);
            gameLayout.Children.Add(roadImage2);
            gameLayout.Children.Add(roadImage3);
            gameLayout.Children.Add(carImage);



        }

        //keyhandler myk

        //int roadLocation = 
        int roady = 0;
        private void gameTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            //UPDATE
            /*if(myk.rightKeyPress){
            player.moveRight();

            }
            */
            roady += 1;
            if(roady > 200)
            {
                roady = 0;
            }


            //REDRAW
            MainThread.BeginInvokeOnMainThread(() =>
            {
               // myImage.TranslationX += 1;
                roadImage1.TranslationY = roady;
                roadImage2.TranslationY = roady;
                roadImage3.TranslationY = roady;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            gameTimer.Stop();
            gameTimer.Dispose();
        }
    }
}
