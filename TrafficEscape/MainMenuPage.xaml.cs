using System.Threading.Tasks;

namespace TrafficEscape
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            int highScore = Preferences.Default.Get("HighScore", 0);
            HighScoreLabel.Text = $"High Score: {highScore}";

            int totalCoins = Preferences.Default.Get("TotalCoins", 0);
            CoinsLabel.Text = $"Coins: {totalCoins}";
        }


        private void StartGame_Clicked(object sender, EventArgs e)
        {
            // Navigate to the game page
            Shell.Current.GoToAsync(nameof(GamePage));

        }
    }
}
