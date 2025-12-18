using Plugin.Maui.Audio;
using System.Threading.Tasks;

namespace TrafficEscape
{
    public partial class MainMenuPage : ContentPage
    {
        //audio player
        IAudioPlayer menuMusic;
        public MainMenuPage()
        {
            InitializeComponent();
        }

        //runs whenever menu page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //load music
            if (menuMusic == null)
            {
                var file = await FileSystem.OpenAppPackageFileAsync("MainMenu.mp3");
                menuMusic = AudioManager.Current.CreatePlayer(file);
                menuMusic.Loop = true;
            }

            bool soundEnabled = Preferences.Get("SoundEnabled", true);

            //check if sound enabled
            if (soundEnabled)
            {
                menuMusic?.Play();
            }
            else
            {
                menuMusic?.Stop();
            }

            //load highscore
            int highScore = Preferences.Default.Get("HighScore", 0);
            HighScoreLabel.Text = $"High Score: {highScore}";

            //load total coins
            int totalCoins = Preferences.Default.Get("TotalCoins", 0);
            CoinsLabel.Text = $"Coins: {totalCoins}";

        }

        //runs when start game is pressed
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
