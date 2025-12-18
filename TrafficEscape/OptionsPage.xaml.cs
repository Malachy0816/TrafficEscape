namespace TrafficEscape
{
    public partial class OptionsPage : ContentPage
    {
        public OptionsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            bool soundEnabled = Preferences.Get("SoundEnabled", true);
            SoundSwitch.IsToggled = soundEnabled;
        }

        //runs when sound switch is toggled
        private void SoundSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("SoundEnabled", e.Value);
        }

        //runs when reset high score button is pressed
        private async void ResetHighScore_Clicked(object sender, EventArgs e)
        {
            ResetOverlay.IsVisible = true;
        }

        //runs when confirm button is pressed
        private async void ConfirmReset_Clicked(object sender, EventArgs e)
        {
            // Reset the saved high score
            Preferences.Set("HighScore", 0);
            ResetOverlay.IsVisible = false;
        }

        private async void CancelReset_Clicked(object sender, EventArgs e)
        {
            ResetOverlay.IsVisible = false;
        }


        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

       

    }
}
