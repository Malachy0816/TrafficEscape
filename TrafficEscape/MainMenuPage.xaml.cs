using Plugin.Maui.Audio;
using System.Threading.Tasks;

namespace TrafficEscape
{
    public partial class MainMenuPage : ContentPage
    {

        IAudioPlayer menuMusic;
        public MainMenuPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (menuMusic == null)
            {
                var file = await FileSystem.OpenAppPackageFileAsync("MainMenu.mp3");
                menuMusic = AudioManager.Current.CreatePlayer(file);
                menuMusic.Loop = true;
            }

            menuMusic?.Play();

            int highScore = Preferences.Default.Get("HighScore", 0);
            HighScoreLabel.Text = $"High Score: {highScore}";

            int totalCoins = Preferences.Default.Get("TotalCoins", 0);
            CoinsLabel.Text = $"Coins: {totalCoins}";

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            menuMusic?.Stop();
        }

        private void StartGame_Clicked(object sender, EventArgs e)
        {
            // Navigate to the game page
            Shell.Current.GoToAsync(nameof(GamePage));

        }
    }
}
