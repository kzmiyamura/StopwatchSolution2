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
        private readonly Stopwatch _stopwatch = new();

        private readonly VisualHost _visualHost;

        // 最新Frame（lockなし）
        private Frame? _latestFrame;

        private CancellationTokenSource? _workerCts;

        public MainWindow()
        {
            InitializeComponent();

            _visualHost = new VisualHost();
            DrawArea.Children.Add(_visualHost);

            // サイズ確定通知（最重要）
            DrawArea.SizeChanged += OnDrawAreaSizeChanged;

            CompositionTarget.Rendering += OnRendering;
        }

        /// <summary>
        /// 描画領域サイズが確定・変更されたとき
        /// 初期Frameを必ず生成する
        /// </summary>
        private void OnDrawAreaSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 初期表示 or リサイズ時に Frame を更新
            UpdateFrame(_stopwatch.Elapsed);

            // ここで即1回描画しておく（Rendering待ちしない）
            if (_latestFrame is not null)
            {
                _visualHost.Render(_latestFrame);
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var frame = _latestFrame;
            if (frame is not null)
            {
                _visualHost.Render(frame);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_stopwatch.IsRunning)
                return;

            _stopwatch.Start();

            _workerCts = new CancellationTokenSource();
            var token = _workerCts.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    UpdateFrame(_stopwatch.Elapsed);

                    // 固定fpsには縛られない最低限の休止
                    Thread.Sleep(1);
                }
            }, token);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!_stopwatch.IsRunning)
                return;

            _stopwatch.Stop();
            _workerCts?.Cancel();
            _workerCts = null;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Stop_Click(sender, e);

            _stopwatch.Reset();
            UpdateFrame(TimeSpan.Zero);
        }

        /// <summary>
        /// Immutable Frame 生成
        /// </summary>
        private void UpdateFrame(TimeSpan time)
        {
            var frame = new Frame(
                time.ToString(@"mm\:ss\.ff"),
                new Size(
                    DrawArea.ActualWidth,
                    DrawArea.ActualHeight)
            );

            Interlocked.Exchange(ref _latestFrame, frame);
        }
    }
}
