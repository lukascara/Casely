using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace CaselyData {
    class CaselyParser {
        public enum PathReportToken {
            History,
            Material,
            Gross,
            Microscopic
        }

    //    public static Tokenizer<PathReportToken> Instance { get; } =
    //       new TokenizerBuilder<PathReportToken>()
    //        .Ignore(Span.WhiteSpace)
    //        .Match(Superpower.Parsers.Character. .EqualTo("MATERIALS:", PathReportToken.Material))
           
      /*  public static class SoftPathResultParser {
            public CaselyData.CaseEntry parsePathReportResultField(string result) {
                CaseEntry ce = new CaseEntry();
                List<String> reportLines = result.Split('\n').ToList();
                for (var i = 0; i < reportLines.Count; i++) {
                    var repLine = reportLines[i];
                    switch(repLine) {
                        case "HISTORY:":
                            string hist = "";
                            i++;
                            break;
                        case "MATERIAL:":
                            break;
                        case "GROSS:":
                            break;
                        case "MICROSCOPIC:":
                            break;
                        default:
                            break;
                    }
                }
                return "tes";
            }
        }*/
    }
}
