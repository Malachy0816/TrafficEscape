namespace TrafficEscape
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();

            // Load saved high score
            int highScore = Preferences.Default.Get("HighScore", 0);
            HighScoreLabel.Text = $"High Score: {highScore}";
        }

        private void StartGame_Clicked(object sender, EventArgs e)
        {
            // Navigate to the game page
            Shell.Current.GoToAsync(nameof(GamePage));

        }
    }
}
