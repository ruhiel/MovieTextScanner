using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    public class Options
    {
        [Option('p', "path", Required = true,
            HelpText = "入力ファイルまたはフォルダ")]
        public string InputPath { get; set; }

        [Option('t', "text", Required = true,
            HelpText = "検索テキスト")]
        public string SearchText { get; set; }

        [Option('i', "interval", Default = 10,
            HelpText = "検索間隔（秒）")]
        public int Interval { get; set; }
    }
}
