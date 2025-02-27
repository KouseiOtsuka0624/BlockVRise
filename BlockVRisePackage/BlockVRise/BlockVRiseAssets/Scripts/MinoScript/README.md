# MinoControllScript.cs
MinoControllScript.cs は、本アプリケーションにおいて最も重要なスクリプトの一つです。
このスクリプトは、VR空間におけるブロック（ミノ）のインタラクティブな操作を実現し、従来のテトリススタイルのゲームプレイをVR体験として最適化する役割を果たします。

## 🎯 MinoControllScript.cs の機能
- インタラクティブコントローラー操作
 - コントローラーを用いた ブロックの移動、回転、掴む動作 を実装。
 - A/Bボタンによる回転操作、掴むアクションによる設置管理 など、直感的な操作が可能。
 - VRコントローラーの振動フィードバック により、操作時の触覚的フィードバックを提供。
- リアルタイムでの正確なブロック管理
 - ブロックが グリッド内で正しく配置されるかを常に検証管理（IsValidMovement()）。
 - ブロックの 積み上げや衝突判定、ゲームオーバー判定 を管理。
 - Grid 配列を用いて リアルタイムのグリッド管理システム による、ブロックの正確な整列やライン消去処理を実装。
- 視覚的なフィードバック
 - 操作可能時間のカウントダウン を TextMeshPro で表示し、視覚的にフィードバック。
 - ブロックが掴まれている間の色変化（黄 → 赤 → 黒） により、プレイヤーに制限時間を知らせる。
