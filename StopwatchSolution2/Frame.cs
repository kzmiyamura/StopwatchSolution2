using System.Windows;

namespace StopwatchSolution2
{
    /// <summary>
    /// 1フレーム分の描画状態
    /// Immutable / UIスレッドは読むだけ
    /// </summary>
    public sealed record Frame(
        string TimeText,
        Size ViewportSize,
        double FontSize
    );
}
