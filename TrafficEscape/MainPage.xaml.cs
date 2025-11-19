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

        Image myImage;
        Image roadImage1;

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


            myImage = new Image
            {
                Source = "player_blue_car.png",
                WidthRequest = 100,
                HeightRequest = 200,
            };

            roadImage1 = new Image
            {
                Source = "road.png",
                WidthRequest = 400,
                HeightRequest = 800,
            };

            AbsoluteLayout.SetLayoutBounds(roadImage1, new Rect(gameLayout.Width / 2 - roadImage1.Width / 2, 0, 400, 800));
            AbsoluteLayout.SetLayoutBounds(myImage, new Rect(100,100 , 100, 200));
            

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
            gameLayout.Children.Add(myImage);
            gameLayout.Children.Add(roadImage1);

            
         
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
            if(roady > 100)
            {
                roady = 0;
            }


            //REDRAW
            MainThread.BeginInvokeOnMainThread(() =>
            {
               // myImage.TranslationX += 1;
                roadImage1.TranslationY = roady;
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
