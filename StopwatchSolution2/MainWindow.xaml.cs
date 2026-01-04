using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StopwatchSolution2
{
    public partial class MainWindow : Window
    {
        private readonly VisualHost _visualHost = new VisualHost();

        private Frame _latestFrame =
            new Frame("00:00.00", new Size(1, 1), 72);

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private CancellationTokenSource? _cts;

        private volatile bool _running;

        public MainWindow()
        {
            InitializeComponent();

            DrawCanvas.Children.Add(_visualHost);

            CompositionTarget.Rendering += OnRendering;

            Loaded += (_, __) =>
            {
                UpdateViewport();
                PublishFrame("00:00.00");
            };

            SizeChanged += (_, __) => UpdateViewport();
        }

        // ============================
        // UI Rendering
        // ============================
        private void OnRendering(object? sender, EventArgs e)
        {
            var frame = _latestFrame;
            _visualHost.Render(frame);
        }

        // ============================
        // Buttons
        // ============================
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_running)
                return;

            _running = true;
            _stopwatch.Start();

            _cts = new CancellationTokenSource();
            Task.Run(() => WorkerLoop(_cts.Token));
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!_running)
                return;

            _running = false;
            _stopwatch.Stop();
            _cts?.Cancel();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _running = false;
            _stopwatch.Reset();
            _cts?.Cancel();

            PublishFrame("00:00.00");
        }

        // ============================
        // Worker Thread
        // ============================
        private void WorkerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_running)
                {
                    var t = _stopwatch.Elapsed;
                    var text = $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
                    PublishFrame(text);
                }

                // CPU を休ませる（高Hzでも破綻しない）
                Thread.Sleep(1);
            }
        }

        // ============================
        // Frame publishing
        // ============================
        private void PublishFrame(string text)
        {
            var size = _visualHost.RenderSize;

            if (size.Width <= 0 || size.Height <= 0)
                size = new Size(1, 1);

            var frame = new Frame(
                text,
                size,
                72   // ← ★ FontSize を必ず指定
            );

            Interlocked.Exchange(ref _latestFrame, frame);
        }

        private void UpdateViewport()
        {
            _visualHost.Width = DrawCanvas.ActualWidth;
            _visualHost.Height = DrawCanvas.ActualHeight;

            PublishFrame(_latestFrame.TimeText);
        }
    }
}
