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

            bool soundEnabled = Preferences.Get("SoundEnabled", true);

            if (soundEnabled)
            {
                menuMusic?.Play();
            }
            else
            {
                menuMusic?.Stop();
            }

            int highScore = Preferences.Default.Get("HighScore", 0);
            HighScoreLabel.Text = $"High Score: {highScore}";

            int totalCoins = Preferences.Default.Get("TotalCoins", 0);
            CoinsLabel.Text = $"Coins: {totalCoins}";

        }

        private async void StartGame_Clicked(object sender, EventArgs e)
        {

            menuMusic?.Stop();
            // Navigate to the game page
            await Shell.Current.GoToAsync(nameof(GamePage));

        }
        private async void Options_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(OptionsPage));
        }

    }
}
