# High Performance WPF Stopwatch

描画パフォーマンスを最優先に設計された WPF ストップウォッチアプリケーションです。  
WPF の一般的な MVVM / Binding / DispatcherTimer 構成をあえて避け、  
**フレーム駆動・Immutable データ・ロックレス設計** によって、  
UI スレッド負荷を極小化しています。

---

## 特徴

- UI スレッドでの計測処理なし
- DispatcherTimer 不使用
- lock 不使用
- 高精度 Stopwatch による時間計測
- 描画はフレーム単位（CompositionTarget.Rendering）
- DrawingVisual による最小描画パス
- Interlocked によるロックレス Frame 受け渡し
- 高リフレッシュレート環境（120Hz / 144Hz）対応

---

## 設計方針

### 基本原則

- **計測と描画を完全に分離**
- **UI スレッドは「読むだけ」**
- **最新状態のみを描画**
- **フレームを落とすことを前提にする**

---

## アーキテクチャ概要
```
┌─────────────┐
│ Worker Thread│
│ │
│ Stopwatch │
│ Frame生成 │
└──────┬──────┘
│ Interlocked.Exchange
┌──────▼──────┐
│ Frame │ (Immutable)
└──────┬──────┘
│ 読み取りのみ
┌──────▼───────────────┐
│ UI Thread │
│ CompositionTarget │
│ Rendering │
│ DrawingVisual描画 │
└──────────────────────┘
```

---

## Frame 設計

```csharp
public sealed record Frame(
    string TimeText,
    Size ViewportSize
);
