using Microsoft.Maui.Graphics;

namespace TrafficEscape
{
    public class RoadDrawable : IDrawable
    {
        public float Offset = 0;   // how far the dashed lines have moved
        public float Speed = 4;    // how fast the road scrolls

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;

            // Background road color
            canvas.FillColor = Colors.DarkGray;
            canvas.FillRectangle(0, 0, width, height);

            canvas.StrokeDashPattern = null;

           canvas.StrokeColor = Colors.Yellow;
           canvas.StrokeSize = 6;

            float borderOffset = 20;

            canvas.DrawLine(borderOffset, 0, borderOffset, height);
            canvas.DrawLine(width - borderOffset, 0, width - borderOffset, height);

            // Dash settings
            canvas.StrokeColor = Colors.White;
            canvas.StrokeDashPattern = new float[] { 10, 10 };
            canvas.StrokeSize = 5;



            // 5 lanes -> 4 dividers
            int laneCount = 5;
            float laneWidth = width / laneCount;

            for (int i = 1; i < laneCount; i++)
            {
                float x = i * laneWidth;

                // Draw first line
                canvas.DrawLine(x, Offset, x, height + Offset);

                // Draw second line above it for seamless looping
                canvas.DrawLine(x, Offset - height, x, Offset);
            }
        }
    }
}


