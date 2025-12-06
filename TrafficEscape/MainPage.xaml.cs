namespace TrafficEscape
{
    public partial class GamePage : ContentPage
    {
        // How far the car moves per click
        const double MoveAmount = 20;

        public GamePage()
        {
            InitializeComponent();
        }

        async void MoveLeft(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX - MoveAmount;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }

        async void MoveRight(object sender, EventArgs e)
        {
            double newX = PlayerCar.TranslationX + MoveAmount;

            await PlayerCar.TranslateTo(newX, PlayerCar.TranslationY, 80, Easing.Linear);
        }
    }
}
