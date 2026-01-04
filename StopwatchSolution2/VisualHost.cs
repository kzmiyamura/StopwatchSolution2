using System.Windows;
using System.Windows.Media;

namespace StopwatchSolution2
{
    /// <summary>
    /// DrawingVisual を保持・描画するためのホスト
    /// </summary>
    public sealed class VisualHost : FrameworkElement
    {
        private readonly VisualCollection _children;
        private readonly DrawingVisual _visual = new();

        public VisualHost()
        {
            _children = new VisualCollection(this)
            {
                _visual
            };
        }

        /// <summary>
        /// Frame内容をそのまま描画する
        /// </summary>
        public void Render(Frame frame)
        {
            if (frame.ViewportSize.Width <= 0 ||
                frame.ViewportSize.Height <= 0)
            {
                return;
            }

            using var dc = _visual.RenderOpen();

            // 背景（黒）
            dc.DrawRectangle(
                Brushes.Black,
                null,
                new Rect(new Point(0, 0), frame.ViewportSize)
            );

            // 文字描画
            var text = new FormattedText(
                frame.TimeText,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                48,
                Brushes.Lime,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

            // 中央寄せ
            var x = (frame.ViewportSize.Width - text.Width) / 2;
            var y = (frame.ViewportSize.Height - text.Height) / 2;

            dc.DrawText(text, new Point(x, y));
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
            => _children[index];
    }
}
