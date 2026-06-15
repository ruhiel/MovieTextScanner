# Movie Text Scanner

動画内の特定の文字列を検索するツールです。

動画を一定間隔で画像化し、画像の右1/3を切り抜いてOCRを実行し、指定した文字列を含むフレームの時刻を表示します。

## 特徴

- 動画を一定間隔で画像化
- 画像の右1/3のみを切り抜き
- YOMITOKUによるOCR
- 指定した文字列を検索
- 該当箇所の時刻を表示
- Spectre.Consoleによる進捗表示

---

## 動作の流れ

1. 動画から一定間隔で画像を抽出
2. 画像の右1/3を切り抜き
3. YOMITOKUでOCRを実行
4. OCR結果(JSON)を読み込み
5. 指定した文字列を検索
6. 一致した時刻を表示

---

## 必要環境

- .NET
- ffmpeg
- ffprobe
- yomitoku

これらのコマンドがPATHに通っている必要があります。

### ffmpeg

https://ffmpeg.org/

### YOMITOKU

https://github.com/kotaro-kinoshita/yomitoku

---

## インストール

```bash
git clone <repository>
```

必要な NuGet パッケージ

- CommandLineParser
- Spectre.Console

---

## 使い方

```bash
MovieTextScanner.exe -i movie.mp4 -s "検索文字列"
```

例

```bash
MovieTextScanner.exe -i sample.mp4 -s "CM"
```

---

## オプション

| オプション | 説明 |
|------------|------|
| -i, --input | 入力動画 |
| -s, --search | 検索文字列 |
| -n, --interval | 画像抽出間隔（秒） |

例

```bash
MovieTextScanner.exe ^
    -i sample.mp4 ^
    -s "提供" ^
    -n 10
```

10秒ごとに画像を抽出して検索します。

---

## 出力例

```text
検索対象：sample.mp4
検索文字列：提供
動画長さ：1:35:42

画像抽出中      ━━━━━━━━━━━━━━━━━━━ 100%
画像切り抜き    ━━━━━━━━━━━━━━━━━━━ 100%
OCR中          ━━━━━━━━━━━━━━━━━━━ 100%

0:12:40 提供：○○株式会社
0:25:10 提供：△△株式会社
1:03:20 提供：□□株式会社

実行時間：0:04:36
完了
```

---

## OCR対象

処理速度向上のため、各フレームの

**右1/3**

のみをOCR対象としています。

```text
┌─────────────────┬──────┐
│                 │ OCR │
│                 │対象 │
│     映像部分     │右1/3│
│                 │     │
└─────────────────┴──────┘
```

テロップや字幕が画面右側に表示される動画を想定しています。

---

## 使用ライブラリ

### Spectre.Console

コンソールUI・進捗表示

https://spectreconsole.net/

### CommandLineParser

コマンドライン引数解析

https://github.com/commandlineparser/commandline

### ffmpeg

動画から画像を抽出

https://ffmpeg.org/

### YOMITOKU

OCR

https://github.com/kotaro-kinoshita/yomitoku

---

## ライセンス

MIT
