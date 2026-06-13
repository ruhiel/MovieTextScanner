using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Drawing;

namespace MovieTextScanner
{
    internal class Program
    {
        static void Proc(Options opt)
        {
            var sw = Stopwatch.StartNew();

            var inputFile = opt.InputPath;
            var temp1 = new TempDirectory();
            var outputPattern = Path.Combine(temp1.GetTempDir(), "frame_%04d.jpg");

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{inputFile}\" -vf fps=1/{opt.Interval} \"{outputPattern}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process.WaitForExit();

            var temp3 = new TempDirectory();

            string inputDir = temp1.GetTempDir();
            string outputDir = temp3.GetTempDir();

            Directory.CreateDirectory(outputDir);

            foreach (var path in Directory.GetFiles(inputDir))
            {
                var src = new Bitmap(path);

                var cropWidth = src.Width / 3;

                // 右1/3の領域
                var rect = new Rectangle(
                    src.Width - cropWidth,
                    0,
                    cropWidth,
                    src.Height);

                var cropped = src.Clone(rect, src.PixelFormat);

                var outputPath = Path.Combine(outputDir, Path.GetFileName(path));
                cropped.Save(outputPath);
            }

            var temp2 = new TempDirectory();

            var psi2 = new ProcessStartInfo
            {
                FileName = "yomitoku",
                Arguments = $"{temp3.GetTempDir()} --lite -o {temp2.GetTempDir()} -f json",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process = Process.Start(psi2);
            process.WaitForExit();

            var jsonList = new List<JsonFile>();

            foreach (var path in Directory.GetFiles(temp2.GetTempDir(), "*.json"))
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

            Console.WriteLine(sw.Elapsed.ToString(@"h\:mm\:ss"));

            Console.WriteLine("完了");

        }

        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            switch (parseResult.Tag)
            {
                // パース成功
                case ParserResultType.Parsed:
                    var parsed = parseResult as Parsed<Options>;
                    Options opt = parsed.Value;
                    Proc(opt);
                    break;

                // パース失敗
                case ParserResultType.NotParsed:
                    // パースの成否でパース結果のオブジェクトの方が変わる
                    var notParsed = parseResult as NotParsed<Options>;

                    break;
            }

        }

    }
}
