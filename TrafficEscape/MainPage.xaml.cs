using TrafficEscape.Game;

namespace TrafficEscape
{
    public partial class MainPage : ContentPage
    {
        private GameEngine _engine;
        private GameDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();

            _engine = new GameEngine(GameView);
            _drawable = new GameDrawable(_engine);
            GameView.Drawable = _drawable;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, EventArgs e)
        {
            await _engine.PreloadImagesAsync();
            _engine.start();
        }
    }
}
