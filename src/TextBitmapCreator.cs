using System.Drawing;

namespace NumberDisplayer.WindowsApplication {
    public interface ITextDrawer {
        Bitmap Create(string text, Font font, Color textColor, Color backColor);
    }

    public class TextBitmapCreator : ITextDrawer {
        // https://stackoverflow.com/a/2070493/116895
        public Bitmap Create(string text, Font font, Color textColor, Color backColor) {
            var result = new Bitmap(1, 1);
            var drawing = Graphics.FromImage(result);
            var textSize = drawing.MeasureString(text, font);
            result.Dispose();
            drawing.Dispose();
            result = new Bitmap((int) textSize.Width, (int) textSize.Height);
            drawing = Graphics.FromImage(result);
            drawing.Clear(backColor);
            Brush textBrush = new SolidBrush(textColor);
            drawing.DrawString(text, font, textBrush, 0, 0);
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            return result;
        }
    }
}