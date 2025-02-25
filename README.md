# BlockVRise（調整中）
BlockVRiseは、Meta Questシリーズ用に作成したVR版テトリスライクゲームです。
従来のテトリスゲームをVR空間でプレイすることが可能であり、コントローラーで直接ミノを掴んだり、動かしたりといったインタラクティブな操作が可能です。
具体的に以下の機能が実装されています。
![BlockVRiseGif](https://github.com/user-attachments/assets/5e5246ad-4f44-4c75-9876-21acab235ddf)

## 🎯 BlockVRise の機能
- **インタラクティブコントローラー操作**
  - コントローラーを用いた **ブロックの移動、回転、掴む動作** を実装。
    - A/Bボタンによる回転操作、掴むアクションによる設置管理 など、直感的な操作が可能。
  - VRコントローラーの **振動フィードバック** により、操作時の **触覚的フィードバック** を提供。
- **リアルタイムでの正確なブロック管理**
  - ブロックが **グリッド内で正しく配置** されるかを常に検証管理。
  - ブロックの **積み上げや衝突判定、ゲームオーバー判定** を管理。
  - Grid 配列を用いて **リアルタイムのグリッド管理システム** による、ブロックの正確な整列やライン消去処理を実装。
- **視覚的なフィードバック**
  - ブロック操作に制限時間を設け、 **操作可能時間のカウントダウン** をTextMeshProで表示し、視覚的にフィードバック。
    - ブロックが掴まれている間の色変化（黄 → 赤 → 黒） により、プレイヤーに制限時間を知らせる。
  
これらの機能の中核を担うのが、BlockVRise/BlockVRiseAssets/Scripts/MinoScript/ **MinoControllScript.cs** です。

また、本アプリケーションは大学院の研究で用いるユーザスタディ用アプリケーションとして作成しました。
本リポジトリは、 **アプリの動作確認や技術的支援、共有のための成果物公開用として作成、公開** されています。

## 🚀公開方法
このアプリケーションは以下の二つの方法で提供されております。

### ① APK ファイルとして提供（実行形式）
Meta Questシリーズに直接インストールして動作確認が可能なAPKファイルを提供します。
#### 📥APKダウンロード
👉[APKファイルダウンロード](https://drive.google.com/file/d/12nlYZyTU77DEyzbyYt3QKizTHL6avole/view?usp=drive_link)
#### 📌インストール手順
1. PC側でSideQuest をインストール
2. MetaQuestシリーズをPCに接続し、開発者モードを有効化
3. SideQuest で APK を Quest に転送
4. Quest の 「不明なソース」 からアプリを実行

### ② Unity パッケージ（必要Assetのみ）として提供
本プロジェクトの主要なシーンおよび関連するScriptなどのコンポーネントのみを Unity パッケージ（.unitypackage） として提供します。
この方法では、Meta XR SDK やその他の外部ライブラリを含めずに、BlockVRise のコア部分のみを共有します。
そのため、Unityエディタでのアプリカスタマイズが可能ですが、いくつかのSDKやコンポーネントを別途インポートする必要があります。

#### 📥Unity パッケージのダウンロード
👉[Unityパッケージファイル](https://github.com/KouseiOtsuka0624/BlockVRise/tree/master/BlockVRisePackage)
からBlockVRisePackage.unitypackageファイルをダウンロード
#### 📌インストール＆動作確認手順（Unity で開く場合）
1. Unity 2021.3 LTS 以上をインストール(本プロジェクトはUnity 2021.3.25f1を使用し作成)
2. Meta XR SDK や XR Plug-in Management など、MetaQuestアプリ開発に必要なパッケージを導入し、環境構築
   - 詳しい環境構築の仕方はこちらのサイトの「Questアプリをビルドするには」を参考 https://tech.framesynthesis.co.jp/unity/metaquest/
4. File > Import Package > Custom Package から BlockVRisePackage.unitypackage をインポートし、インポートファイル内のBlockVRiseシーンを開く
5. OVRCameraRigを設置する。
   - 上記サイトの「vrのカメラ配置の基本」を参考
6. OVRCameraRig＞TrackingSpace＞RightHandAnchor＞RightControllerAnchorの子にインポートしたフォルダ内のRayObjectを設置
7. 6で設置したRayObjectをGameManagerオブジェクトにアタッチされているGameManagementScriptコンポーネントのLineRenderとLazerPointerにアタッチする。
8. 同様のGameManagementScriptコンポーネントのRightControllerにOVRCameraRig＞TrackingSpace＞RightHandAnchor＞RightControllerAnchorをアタッチする。
9. OVRCameraRig＞TrackingSpace＞RightHandAnchor＞LeftControllerAnchorをMinoSponerオブジェクトにアタッチされているSpawnMinoコンポーネントのLeftControllerにアタッチする。
10. PCと開発者モードに設定したMetaQuestシリーズを接続し、ビルド＆ラン

## 🛠 使用技術
- Unity 2021.3.25f1
- C#
- Meta XR SDK（Oculus Integration）
- XR Plug-in Management
- DearVR Unity（3Dオーディオ処理）

## 📜 ライセンス & 注意点
Meta XR SDKやDearVR Unityなどアプリケーション実装のためにインポートしたAssetは、本リポジトリに含まれていません。Unityパッケージ版を使用する場合、必要なAssetを別途インポートしてください。
本リポジトリは **技術的支援および成果物公開用であり、商用利用を目的としていません** 。

## 🎥 デモ動画
[Youtubeリンク
](https://youtu.be/d81Epoe6ncE)
## 📩 お問い合わせ
本プロジェクトに関するお問い合わせは以下で受け付けています。(メールの方が確実です)
- GitHub の Issues
- メール
  - [個人用] kouseiotsuka0624@gmail.com
  - [大学用] 6524002o@st.toho-u.ac.jp
 
## リポジトリツリー
    BlockVRisePackage/
    ├── BlockVRise/                           # BlockVRisePackage.unitypackage に含まれているAssetフォルダ
    └── BlockVRisePackage.unitypackage        # BlockVRiseのパッケージファイル（「② Unity パッケージ（必要Assetのみ）として提供」で使用）
    BlockVRiseApk.apk                         # BlockVRiseのapkファイル（「① APK ファイルとして提供（実行形式）」で使用）



## BGM, SE提供
本アプリケーションで使用しているBGMおよびSEは **フリーBGM・音楽素材MusMus** 様より提供していただいたものです。
https://musmus.main.jp
