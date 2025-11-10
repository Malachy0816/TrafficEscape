using Microsoft.Maui.Graphics;


namespace TrafficEscape.Game
{
    public class GameDrawable : IDrawable
    {

        public static GameDrawable Instance { get; } = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);
            canvas.FontColor = Colors.White;
            canvas.DrawString("Traffic Escape", dirtyRect.Center.X, dirtyRect.Center.Y, HorizontalAlignment.Center);
        }
    }
}
