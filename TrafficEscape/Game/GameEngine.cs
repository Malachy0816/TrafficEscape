using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace TrafficEscape.Game
{
    public class GameEngine
    {
        private readonly GraphicsView _view;

        public float RoadOffset { get; private set; } = 0f;

        public float Speed { get; private set; } = 300f;

        public Microsoft.Maui.Graphics.IImage RoadImage { get; private set; }

        private bool _running = false;
        private System.Diagnostics.Stopwatch _stopwatch = new();

        public GameEngine(GraphicsView view)
        {
            _view = view;
        }

        public async Task PreloadImagesAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("road.png"); // file not being found
            RoadImage = PlatformImage.FromStream(stream);
        }

        public void start()
        {
            if (_running) return;
            _running = true;
            _stopwatch.Start();

            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) =>
            {
                float dt = (float)_stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Restart();
                Update(dt);
                _view.Invalidate(); // triggers redraw
            };
            timer.Start();
        }

        private void Update(float dt)
        {
            if (RoadImage == null) return;
            RoadOffset += Speed * dt;
            RoadOffset %= RoadImage.Height;
        }
    }
}
