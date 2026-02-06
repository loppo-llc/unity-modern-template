# UnityModernTemplate

[English version](README.md)

**外部 .NET プロジェクト**から Unity 用 DLL をビルドするテンプレート。モダンな C# 開発、高速イテレーション、Unity Editor 外でのテスト駆動開発を実現します。

## なぜ外部 .NET プロジェクト？

Unity 組み込みのスクリプティング (asmdef) には制約があります：

- **最新 .NET API が使えない** — Unity のコンパイラは主流の .NET に追従しきれない
- **イテレーションが遅い** — コード変更のたびに Editor でドメインリロードが発生
- **TDD がしにくい** — Unity Test Framework は Editor の起動が必要
- **AI エージェントと相性が悪い** — AI コーディングエージェントが Unity なしでビルド・テストしにくい

このテンプレートは標準的な .NET プロジェクトでコードをコンパイルし、生成された DLL を Unity に自動コピーすることで、これらすべてを解決します。

## プロジェクト構成

```
UnityModernTemplate/
├── unity/                              # Unity 6 (6000.3.7f1) プロジェクト
│   └── Assets/_Main/Scripts/           # ポストビルドで DLL が自動コピーされる
│
├── csharp/                             # .NET ソリューション
│   ├── Directory.Build.props           # Unity パス解決 + 共有設定
│   ├── Directory.Build.targets         # ポストビルドコピー処理
│   ├── src/
│   │   ├── UnityDotNetSample/          # ランタイムライブラリ → csharp.dll
│   │   │   ├── Components/             #   MonoBehaviour サンプル
│   │   │   ├── Core/                   #   純粋 C# ユーティリティ
│   │   │   └── ScriptableObjects/      #   ScriptableObject サンプル
│   │   │
│   │   └── EditorConsoleLogProxy/      # Editor ライブラリ → editor-console-log-proxy.dll
│   │                                   #   Unity コンソールログをファイルに中継
│   │
│   └── tests/
│       └── UnityDotNetSample.Tests/    # xUnit テスト (net9.0)
│
├── .editorconfig                       # コードスタイルルール
└── .gitignore
```

## 前提条件

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Unity 6 (6000.3.x)](https://unity.com/releases/editor/whats-new/6000.3.7)

## 使い方

### ビルド

```bash
cd csharp
dotnet build
```

すべてのプロジェクトがコンパイルされ、DLL が `unity/Assets/_Main/Scripts/` に自動コピーされます。

### テスト

```bash
dotnet test
```

28 件のテストが xUnit により Unity 外で実行されます — Editor 不要。

### Unity で開く

Unity Hub で `unity/` を開きます。ビルド済みの DLL がすでに配置されています。

## DLL 自動コピーの仕組み

1. 各 `.csproj` で `<CopyToUnityPlugins>true</CopyToUnityPlugins>` を設定
2. `Directory.Build.targets` のポストビルドステップが DLL + PDB を `Assets/_Main/Scripts/` にコピー
3. Unity がファイル変更を検知し、変更された DLL のみを再インポート

ランタイムコードの編集はランタイム DLL のみ、Editor コードの編集は Editor DLL のみリビルド — **最小限のドメインリロード**。

## Unity パス設定

Unity Editor のパスは 3 段階で解決されます（優先度の高い順）：

1. **環境変数**: `UNITY_EDITOR_PATH`
2. **ローカルユーザーファイル**: `Directory.Build.props.user`（gitignore 済み）
3. **デフォルト値**: `C:\Program Files\Unity\Hub\Editor\6000.3.7f1`

## 新しいプロジェクトの追加方法

1. `src/` 以下に新しいフォルダを作成（例: `src/MyFeature/`）
2. `netstandard2.1` ターゲットの `.csproj` を作成
3. `<CopyToUnityPlugins>true</CopyToUnityPlugins>` を設定
4. Unity モジュール参照を `<Private>false</Private>` で追加
5. `csharp.slnx` にプロジェクトを追加
6. テストプロジェクトに `<ProjectReference>` を追加
7. `.gitignore` に新しい DLL/PDB/meta のエントリを追加

## EditorConsoleLogProxy

Unity コンソール出力を `Logs/console.log` に JSON Lines 形式で書き出す組み込み Editor 拡張。AI エージェントや外部ツールが Editor UI を開かずに Unity ログを読み取れるようにします。

- `[InitializeOnLoad]` で自動初期化
- `FileShare.Read` によるスレッドセーフなファイル書き込み（同時読み取り対応）
- ドメインリロードのたびにファイルをクリア

ログ形式：
```json
{"t":"2025-02-06T23:45:12.345Z","l":"Log","m":"Hello World"}
{"t":"2025-02-06T23:45:12.346Z","l":"Error","m":"NullRef","s":"at Foo.Bar()"}
```

## 技術的な注意事項

### CS0433 回避 — UnityEngine.dll（ファサード）を参照しないこと

**モジュール DLL**（`UnityEngine.CoreModule.dll` など）のみを参照し、ファサード `UnityEngine.dll` は絶対に参照しないこと。両方を参照すると `CS0433: 型が両方のアセンブリに存在します` エラーが発生します。

### Assets/Plugins/ を避ける

外部 DLL を `Assets/Plugins/` に配置**しないこと**。Unity がこのフォルダを特別扱いし、DLL を Roslyn アナライザーとして解釈しようとして誤った警告が出る場合があります。特殊でないフォルダ（例: `Assets/_Main/Scripts/`）を使用してください。

### ImplicitUsings = disable

`System.Object` と `UnityEngine.Object` の曖昧さを避けるために必要です。両方の名前空間が暗黙的にインポートされると衝突します。

## ライセンス

[MIT LICENSE](LICENSE)
