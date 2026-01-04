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

        // Sizeを包む参照型（volatile可能）
        private volatile ViewportInfo _viewport =
            new ViewportInfo { Size = new Size(1, 1) };

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
        // UI Rendering (UIスレッド)
        // ============================
        private void OnRendering(object? sender, EventArgs e)
        {
            _visualHost.Render(_latestFrame);
        }

        // ============================
        // Buttons
        // ============================
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_running) return;

            _running = true;
            _stopwatch.Start();

            _cts = new CancellationTokenSource();
            Task.Run(() => WorkerLoop(_cts.Token));
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!_running) return;

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
                    var text =
                        $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";

                    PublishFrame(text);
                }

                Thread.Sleep(1);
            }
        }

        // ============================
        // Frame publishing
        // ============================
        private void PublishFrame(string text)
        {
            var viewport = _viewport; // volatile read

            var frame = new Frame(
                text,
                viewport.Size,
                72
            );

            Interlocked.Exchange(ref _latestFrame, frame);
        }

        // ============================
        // UIスレッド専用
        // ============================
        private void UpdateViewport()
        {
            var size = new Size(
                DrawCanvas.ActualWidth,
                DrawCanvas.ActualHeight
            );

            if (size.Width <= 0 || size.Height <= 0)
                size = new Size(1, 1);

            _visualHost.Width = size.Width;
            _visualHost.Height = size.Height;

            // 新しい参照を丸ごと差し替え（安全）
            _viewport = new ViewportInfo { Size = size };

            PublishFrame(_latestFrame.TimeText);
        }

        // ============================
        // helper
        // ============================
        private sealed class ViewportInfo
        {
            public Size Size;
        }
    }
}
