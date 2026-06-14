using CommandLine;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace MovieTextScanner
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            switch (parseResult.Tag)
            {
                // パース成功
                case ParserResultType.Parsed:
                    var parsed = parseResult as Parsed<Options>;
                    var opt = parsed.Value;
                    Console.WriteLine($"検索対象：{opt.InputPath}");
                    Console.WriteLine($"検索文字列：{opt.SearchText}");
                    Proc(opt);
                    return 0;

                // パース失敗
                case ParserResultType.NotParsed:
                    var notParsed = (NotParsed<Options>)parseResult;

                    foreach (var error in notParsed.Errors)
                    {
                        Console.Error.WriteLine(error);
                    }

                    return 1;

                default:
                    return 1;
            }

        }
        static double GetDuration(string path)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe.exe",
                Arguments =
                    $"-v error -show_entries format=duration " +
                    "-of default=noprint_wrappers=1:nokey=1 " +
                    $"\"{path}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return double.Parse(output, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        static void Proc(Options opt)
        {
            var sw = Stopwatch.StartNew();

            var inputFile = opt.InputPath;
            var temp1 = new TempDirectory();
            var temp2 = new TempDirectory();
            var temp3 = new TempDirectory();
            var outputPattern = Path.Combine(temp1.GetTempDir(), "frame_%04d.jpg");

            // 動画の長さ（秒）
            var duration = GetDuration(inputFile);

            Console.WriteLine($"動画長さ：{TimeSpan.FromSeconds(duration).ToString(@"h\:mm\:ss")}");

            var totalFrames = (int)Math.Ceiling(duration / opt.Interval);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{inputFile}\" -vf fps=1/{opt.Interval} \"{outputPattern}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            AnsiConsole.Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn())
                .Start(ctx =>
                {
                    ExtractFrames(ctx, psi, temp1, totalFrames);

                    CropImages(ctx, temp1, temp2);

                    RunOcr(ctx, temp1, temp2, temp3);
                });

            var jsonList = new List<JsonFile>();

            foreach (var path in Directory.GetFiles(temp3.GetTempDir(), "*.json"))
            {
                using (var stream = File.OpenRead(path))
                {
                    var data = JsonSerializer.Deserialize<RootObject>(stream);

                    if (data != null)
                    {
                        var json = new JsonFile() { Data = data, FileName = Path.GetFileName(path) };
                        jsonList.Add(json);
                    }
                }
            }

            foreach (var json in jsonList)
            {
                var first = json.Data.Words.Where(x => x.Content.Contains(opt.SearchText)).FirstOrDefault();

                if (first != null)
                {
                    var match = Regex.Match(json.FileName, @"frame_(\d+)_");
                    if (match.Success)
                    {
                        var frameNo = int.Parse(match.Groups[1].Value);

                        // 10倍して秒数にする
                        var totalSeconds = frameNo * 10;

                        // 時:分:秒に変換
                        var time = TimeSpan.FromSeconds(totalSeconds);

                        var result = time.ToString(@"h\:mm\:ss");

                        Console.WriteLine($"{result} {first.Content}");
                    }
                }
            }
            sw.Stop();

            Console.WriteLine($"実行時間：{sw.Elapsed.ToString(@"h\:mm\:ss")}");

            Console.WriteLine("完了");

        }

        private static void RunOcr(ProgressContext ctx, TempDirectory temp1, TempDirectory temp2, TempDirectory temp3)
        {
            var inputDir = temp1.GetTempDir();
            var files = Directory.GetFiles(inputDir);
            //
            // OCR
            //
            var psi2 = new ProcessStartInfo
            {
                FileName = "yomitoku",
                Arguments = temp2.GetTempDir() +
                            " --lite -o " +
                            temp3.GetTempDir() +
                            " -f json",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var totalFiles = files.Length;

            var ocrTask = ctx.AddTask("OCR中", maxValue: totalFiles);

            using (var process2 = Process.Start(psi2))
            {
                var current = 0;

                while (!process2.HasExited)
                {
                    var count = Directory.GetFiles(
                        temp3.GetTempDir(),
                        "*.json").Length;

                    if (count > current)
                    {
                        ocrTask.Increment(count - current);
                        current = count;
                    }

                    Thread.Sleep(500);
                }

                var finalCount = Directory.GetFiles(
                    temp3.GetTempDir(),
                    "*.json").Length;

                if (finalCount > current)
                {
                    ocrTask.Increment(finalCount - current);
                }

                process2.WaitForExit();
            }
        }
        private static void CropImages(ProgressContext ctx, TempDirectory temp1, TempDirectory temp2)
        {
            //
            // 画像切り抜き
            //
            var inputDir = temp1.GetTempDir();
            var outputDir = temp2.GetTempDir();

            Directory.CreateDirectory(outputDir);

            var files = Directory.GetFiles(inputDir);

            var cropTask = ctx.AddTask("画像切り抜き", maxValue: files.Length);

            foreach (var path in files)
            {
                cropTask.Description =
                    "切り抜き中: " + Path.GetFileName(path);

                using (var src = new Bitmap(path))
                {
                    var cropWidth = src.Width / 3;

                    var rect = new Rectangle(
                        src.Width - cropWidth,
                        0,
                        cropWidth,
                        src.Height);

                    using (var cropped = src.Clone(rect, src.PixelFormat))
                    {
                        var outputPath =
                            Path.Combine(outputDir, Path.GetFileName(path));

                        cropped.Save(outputPath);
                    }
                }

                cropTask.Increment(1);
            }
        }

        private static void ExtractFrames(ProgressContext ctx, ProcessStartInfo psi, TempDirectory temp1, int totalFrames)
        {
            //
            // 画像抽出
            //
            var extractTask = ctx.AddTask("画像抽出中", maxValue: totalFrames);

            using (var process1 = Process.Start(psi))
            {
                var current = 0;

                while (!process1.HasExited)
                {
                    var count = Directory.GetFiles(temp1.GetTempDir(), "*.jpg").Length;

                    if (count > current)
                    {
                        extractTask.Increment(count - current);
                        current = count;
                    }

                    Thread.Sleep(500);
                }

                var finalCount = Directory.GetFiles(temp1.GetTempDir(), "*.jpg").Length;

                if (finalCount > current)
                {
                    extractTask.Increment(finalCount - current);
                }

                process1.WaitForExit();
                extractTask.Value = extractTask.MaxValue;
            }
        }
    }
}
