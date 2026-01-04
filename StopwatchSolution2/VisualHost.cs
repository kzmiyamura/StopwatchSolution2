using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace StopwatchSolution2
{
    /// <summary>
    /// DrawingVisual をホストする FrameworkElement
    /// 描画は CompositionTarget.Rendering から呼ばれる
    /// </summary>
    public sealed class VisualHost : FrameworkElement
    {
        private readonly DrawingVisual _visual = new DrawingVisual();

        private Frame? _currentFrame;

        /// <summary>
        /// 7セグ風デジタルフォント
        /// </summary>
        private static readonly Typeface DigitalTypeface =
            new Typeface(
                new FontFamily(
                    new Uri("pack://application:,,,/"),
                    "./Fonts/#DSEG7 Classic"
                ),
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Normal
            );

        public VisualHost()
        {
            AddVisualChild(_visual);
            AddLogicalChild(_visual);
        }

        /// <summary>
        /// UIスレッドから呼ばれる描画関数
        /// </summary>
        public void Render(Frame frame)
        {
            if (ReferenceEquals(_currentFrame, frame))
                return;

            _currentFrame = frame;

            using var dc = _visual.RenderOpen();

            DrawBackground(dc, frame.ViewportSize);
            DrawTimeText(dc, frame);
        }

        private static void DrawBackground(DrawingContext dc, Size size)
        {
            dc.DrawRectangle(
                Brushes.Black,
                null,
                new Rect(0, 0, size.Width, size.Height)
            );
        }

        private static void DrawTimeText(DrawingContext dc, Frame frame)
        {
            var text = new FormattedText(
                frame.TimeText,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                DigitalTypeface,
                frame.FontSize,
                Brushes.Lime,
                1.0
            );

            double x = (frame.ViewportSize.Width - text.Width) * 0.5;
            double y = (frame.ViewportSize.Height - text.Height) * 0.5;

            dc.DrawText(text, new Point(x, y));
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _visual;
        }
    }
}
