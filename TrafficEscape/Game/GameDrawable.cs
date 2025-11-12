using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;


namespace TrafficEscape.Game
{
    public class GameDrawable : IDrawable
    {
        private readonly GameEngine _engine;

        public GameDrawable(GameEngine engine)
        {
            _engine = engine;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            if (_engine.RoadImage != null)
            {
                DrawTiledRoad(canvas, _engine.RoadImage, dirtyRect, _engine.RoadOffset);
            }
        }

        private void DrawTiledRoad(ICanvas canvas, Microsoft.Maui.Graphics.IImage image, RectF area, float offset)
        {
            float imgW = image.Width;
            float imgH = image.Height;

            float scale = area.Width / imgW;
            float drawH = imgH * scale;

            float startY = -(offset % drawH);

            for (float y = startY; y < area.Height; y += drawH)
            {
                canvas.DrawImage(image, 0, y, area.Width, drawH);
            }
        }
    }
}
