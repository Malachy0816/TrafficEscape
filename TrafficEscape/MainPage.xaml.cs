

using TrafficEscape.Game;

namespace TrafficEscape
{
    public partial class MainPage : ContentPage
    {
        private GameEngine _engine;

        public MainPage()
        {
            InitializeComponent();
            _engine = new GameEngine(GameView);
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await _engine.PreloadImagesAsync();
            GameView.Drawable = new GameDrawable(_engine);
            _engine.start();
        }
    }
}
