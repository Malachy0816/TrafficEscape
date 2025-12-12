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

        private void SoundSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("SoundEnabled", e.Value);
        }

        private async void ResetHighScore_Clicked(object sender, EventArgs e)
        {
            ResetOverlay.IsVisible = true;
        }

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
