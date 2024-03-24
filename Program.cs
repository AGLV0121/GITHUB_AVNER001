
using System;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SERIALIZATION;
using COREUTIL;

namespace ConsoleAppSIMILARITY
{
    class Program
    {
        static string _DIRPREFIX = @"F:\____MODELS\";
        static string _RESULTSPREFIX = _DIRPREFIX + @"RESULTS\";
        static string _INBASEDIR = _DIRPREFIX + @"\DATA\";

        static Regex rexLexEx = new Regex(@"([\-])((Nou|Prop|Adje|Ver|Adv|Conj|Prepo|Arti|Deter|Phr|Lett|Symb|Pref|Coll|Num|Pron|Inter|Contr|Term|Prov|Punct|Liga).*)([;])", RegexOptions.Compiled);
        //static Regex rexLex =      new Regex(@"[\-](Nou|Prop|Adje|Ver|Adv|Conj|Prepo|Arti|Deter|Phr|Lett|Symb|Pref|Coll|Num|Pron|Inter|Contr|Term|Prov|Punct|Liga)", RegexOptions.Compiled);

        //blackstart-Noun;{{
        static void TestAggregate()
        {
            string file = _INBASEDIR + "AllProcessed_.txt";
            List<string> _lines = File.ReadAllLines(file)
                .Select(x => ModifyProcessedString(x))
                .ToList();

            int a = 0;
            ++a;

            var output =
                _lines
                // split outer separator 
                .Select(line => (First: line.Split('╧')[0], Second: line.Substring(line.IndexOf('╧') + 1, line.IndexOf('╤') - line.IndexOf('╧') - 1), Suffix: line))
                .GroupBy(x => x.First)
                .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Second).ToDictionary(x => x.Key, x => x.Select(y => y.Suffix).ToList()));

            KDictionaryStringDictionaryStringListString ksdls = new(output);
            string fileOut = _RESULTSPREFIX + "SERIALIZED_SIMILARITY_SCORES_.txt";
            ksdls.SerializeEx(fileOut);
            fileOut = _RESULTSPREFIX + "_SIMILARITY_SCORES_FROM_FILE.txt";
            Util.PrintOut(ref output, fileOut, 1);
        }



        static string ModifyProcessedString(string line)
        {
            Match match = rexLexEx.Match(line);

            string s0 = match.Groups[0].Value;
            string s1 = match.Groups[1].Value;
            string s2 = match.Groups[2].Value;
            string s3 = match.Groups[3].Value;
            string s4 = match.Groups[4].Value;

            int is1 = match.Groups[0].Index;
            int is4 = line.Substring(is1).IndexOf(";");

            string result = line.Substring(is1, is4 + 1);
            string result1 = result.Replace("-", "╧"); // ╧╤
            result1 = result1.Replace(";", "╤");
            line = Util.ReplaceFirst(line, result, result1);
            return line;
        }


        static void TesResultsFromSimilarity()
        {
            //string file = _INBASEDIR + "SERIALIZED_SIMILARITY_SCORES_.txt";
            //KDictionaryStringDictionaryStringListString ksdls = new();
            //ksdls = ksdls.DeserializeDynamic(file);
            //Dictionary<string, Dictionary<string, List<string>>> _dict = ksdls.GetDictionaryDictionaryStringListString();
            string file = _INBASEDIR + "_SIMILARITY_SCORES_FROM_FILE.txt";
            List<string> _list = File.ReadAllLines(file)
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(x => !x.StartsWith("---"))
                .ToList();
        }


        static void CompareProcessedAndMaybeMissing()
        {
            int pos;

            string fileIn = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\" + "AllProceesed_.txt",
                line,
                tmp;

            List<string> _processed = File.ReadAllLines(fileIn).ToList(),
                _listErrors = new();

            for (int i = 0; i < _processed.Count; i++)
            {
                line = _processed[i];
                pos = line.IndexOf(";#");
                if ((pos == -1))
                    pos = line.IndexOf(";{");

                if ((pos == -1))
                {
                    _listErrors.Add(line);
                    continue;
                }

                tmp = line.Substring(0, pos);

                pos = tmp.LastIndexOf("-");
                if (pos > -1)
                    _processed[i] = tmp.Substring(0, pos);
            }

            _processed = _processed.Distinct().ToList();

            string fileOut = "AllProcessed_Words.txt";
            Util.PrintOut(ref _processed, fileOut);

            fileIn = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\" + "allWords_.txt";
                List<string> _words = File.ReadAllLines(fileIn).ToList();

            List<string> _missing = MyExceptList(_words, _processed, StringComparison.Ordinal);
            fileOut = "MISSING.txt";
            Util.PrintOut(ref _missing, fileOut);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_listA"></param>
        /// <param name="_listB"></param>
        /// <param name="sc"></param>
        /// <returns></returns>
        static List<string> MyExceptList(List<string> _listA, List<string> _listB, StringComparison sc = StringComparison.Ordinal, int withChoice = 0)
        {
            if (withChoice > 0)
            {
                int A, B;

                List<string> _tmp = new();

                A = _listA.Count;

                B = _listB.Count;

                if (B > A)
                {
                    _tmp = new(_listA);

                    _listA = new(_listB);

                    _listB = _tmp;
                }
            }
            else
            {
                _listA = new(_listA);
                _listB = new(_listB);
            }

            string lineA,
                lineB;

            for (int i = 0; i < _listA.Count; i++)
            {
                lineA = _listA[i];

                for (int j = 0; j < _listB.Count; j++)
                {
                    lineB = _listB[j];

                    if (lineA.Equals(lineB))
                    {
                        _listA[i] = "";
                        break;
                    }
                }
            }

            return _listA.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        static void GetProcessedWords()
        {
            string dir = _DIRPREFIX + @"\RESULTS\DONE";
            string[] files = Directory.GetFiles(dir);
            string line;
            List<string> _listOut = new(),
                _listOutRaw = new(),
                _error = new(),
                _errorBis = new();

            foreach (string file in files)
            {
                _listOut.AddRange(File.ReadAllLines(file).ToList());
            }

            _listOut = _listOut.Distinct().ToList();

            for (int i = 0; i < _listOut.Count; i++)
            {
                line = _listOut[i];

                char c = line[line.Length - 1];
                if (!char.IsDigit(c))
                {
                    _errorBis.Add(line);
                }

                files = Regex.Split(line, "SIM#");

                if(files.Length < 2 )
                {
                    _error.Add(line);
                    continue;
                }

                _listOutRaw.Add(line);

                line = files[1] + "SIM#" + files[0];

                _listOut[i] = line;            
            }

            _listOut = _listOut.Distinct().ToList();
            _listOut.Sort();

            string fileOut = _RESULTSPREFIX + @"AllProcessed_1_GOOD_.txt";
            Util.PrintOut(ref _listOutRaw, fileOut);

            fileOut = _RESULTSPREFIX + @"AllProcessed_1_GOOD_REVERSED_.txt";
            Util.PrintOut(ref _listOut, fileOut);

            fileOut = _RESULTSPREFIX + @"LINES_WITHOUT_SIM_.txt";
            Util.PrintOut(ref _error, fileOut);

            fileOut = _RESULTSPREFIX + @"LINES_NOT_ENDING_WITH_DIGIT_.txt";
            Util.PrintOut(ref _errorBis, fileOut);

            //

            var output =
            _error
            // split outer separator 
            .Select(line => (First: line.Split(';')[0], Suffix: line))
            .GroupBy(x => x.First)
            .ToDictionary(g => g.Key, g => g.GroupBy(x => x.First).ToDictionary(x => x.Key, x => x.Select(y => y.Suffix).ToList()));

            KDictionaryStringDictionaryStringListString ksdls = new(output);

            //output = output.Where(x=>x.IndexOf("$$$") == -1)).ToList();

            //output = output.Where(x => x[0]!='-') && x.IndexOf("$$$") == -1).ToList();

            fileOut = _RESULTSPREFIX + @"SERIALIZED_COMPLEMENT_1_.txt";
            ksdls.SerializeEx(fileOut);


            fileOut = _RESULTSPREFIX + @"MISSING_INTER.txt";
            Util.PrintOut(ref output, fileOut, 1);

            var alist = File.ReadAllLines(fileOut)
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(x => x[0] == '-' || x.IndexOf("$$$") > -1)
                .ToList();

            fileOut = _RESULTSPREFIX + @"MISSING_SIM_READY_FOR PROCESS.txt";

            Util.PrintOut(ref alist, fileOut);
        }

        /// <summary>
        /// 
        /// </summary>
        static void GetTotalWords() // 88745 (with > 1 lex class) - but 81526 words
        {
            Match match;
            bool isnextword;
            string file = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\" + "ForAISimilarityAndClassification_1_.txt";
            
            List<string> _words = File.ReadAllLines(file)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim())// && !x.StartsWith("----")
                .ToList(),
                _listOut = new(),
                _listOutAllwords = new(),
                _listErrors = new();
            ;

            string tmp = "";
            int pos;
            isnextword = false;

            foreach (string word in _words)
            {
                if (word.StartsWith("----"))
                {
                    isnextword = true;
                    continue;
                }

                if (isnextword)
                {
                    isnextword = false;
                    pos = word.IndexOf(";#");
                    if ((pos == -1))
                        pos = word.IndexOf(";{");

                    if ((pos == -1))
                    {
                        _listErrors.Add(word);
                        continue;
                    }

                    tmp = word.Substring(0, pos);
                    _listOutAllwords.Add(tmp);

                    pos = tmp.LastIndexOf("-");
                    if (pos > -1)
                        _listOut.Add(word.Substring(0, pos));
                    else
                        _listErrors.Add(word);

                    continue;
                }

                //match = rexLex.Match(word);
                //if ((match.Success))
                //{
                //    {
                //        _listOut.Add(word.Substring(0, match.Index));
                //    }
                //}
            }




            file = "allWords_WITHLEXCLASS.txt";
            _listOutAllwords = _listOutAllwords.Distinct().ToList();
            Util.PrintOut(ref _listOutAllwords, file);

            file = "allWords_.txt";
            _listOut = _listOut.Distinct().ToList();
            Util.PrintOut(ref _listOut, file);

            file = "errors_.txt";
            Util.PrintOut(ref _listErrors, file);
        }

        //###
        static string quant = "13"; 
        //static string fileToStart = "ForAISimilarityAndClassification_ AFTER_intestino_";
        static string fileToStart = @"ForAISimilarityAndClassification_"+ quant+ "_"; // Test_1_

        
        public static String readFileAsUtf8(string fileName)
        {
            Encoding encoding = Encoding.Default;
            String original = String.Empty;

            using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
            {
                original = sr.ReadToEnd();
                encoding = sr.CurrentEncoding;
                sr.Close();
            }

            if (encoding == Encoding.UTF8)
                return original;

            byte[] encBytes = encoding.GetBytes(original);
            byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        static List<List<string>> GetListOfEntries(string file)
        {
            List<List<string>> _llists = new(),
                _llistsTmp = new();

            //String allThisFile = File.ReadAllText(file, Encoding.Default);
            String allThisFile = readFileAsUtf8(file);

            List<string> _lexes = Regex.Split(allThisFile, @"------")
                .ToList(),
                _lexesLines = new();

            foreach (string line in _lexes)
            {
                _lexesLines = line.Trim().Split(System.Environment.NewLine)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if (_lexesLines.Count > 0)
                    _llists.Add(new List<string>(_lexesLines));
            }

            return _llists;
        }


        //static List<string> sentences = new List<string>
        //    {
        //        "This is the first sentence.",
        //        "This is a somewhat similar sentence.",
        //        "This sentence is quite different.",
        //        "But this one is similar to the previous one."
        //    };


        /// <summary>
        /// process
        /// </summary>
        /// <param name="i"></param>
        /// <param name="_llists"></param>
        /// <returns></returns>
        //static List<string> GetNextSetOfSentences(int i, List<List<string>> _llists)
        //{
        //    return _llists[i];
        //}

        static void FillDictChar(ref Dictionary<char,string> _dict, char charA, string stringB)
        {
            //string sTmp;

            if (!_dict.ContainsKey(charA))
                _dict.Add(charA, stringB);
        }
        static Dictionary<char, string> FillChars()
        {
            Dictionary<char, string> _dc = new();
            FillDictChar(ref _dc, 'à', "a");
            FillDictChar(ref _dc, 'á', "a");
            FillDictChar(ref _dc,'â', "a");
            FillDictChar(ref _dc, 'ä', "a");
            FillDictChar(ref _dc, 'á', "a");
            FillDictChar(ref _dc, 'å', "a");
            FillDictChar(ref _dc, 'Æ', "a");
            FillDictChar(ref _dc,'ä', "a");
            FillDictChar(ref _dc,'é', "e");
            FillDictChar(ref _dc,'è', "e");
            FillDictChar(ref _dc,'ê', "e");
            FillDictChar(ref _dc,'ì', "i");
            FillDictChar(ref _dc,'î', "i");
            FillDictChar(ref _dc,'ï', "i");
            FillDictChar(ref _dc, 'ô', "o");
            FillDictChar(ref _dc, 'ö', "o");
            FillDictChar(ref _dc, 'ó', "o");
            FillDictChar(ref _dc, 'ù', "u");
            FillDictChar(ref _dc, 'α', "a");
            FillDictChar(ref _dc, 'α', "a");
            FillDictChar(ref _dc, 'ñ', "n");
            FillDictChar(ref _dc, 'É', "E");
            FillDictChar(ref _dc, '½', ".1/2");
            FillDictChar(ref _dc, '≡', "=");
            FillDictChar(ref _dc, 'í', "i");
            FillDictChar(ref _dc, 'ɑ', "a");
            FillDictChar(ref _dc, 'Ɑ', "A");
            FillDictChar(ref _dc, 'Ɑ', "A");
            FillDictChar(ref _dc, 'β', "b");
            FillDictChar(ref _dc, 'γ', "g");
            FillDictChar(ref _dc, '-', "-");
            FillDictChar(ref _dc, '²', "2");
            FillDictChar(ref _dc, '∞', "0x9E");
            FillDictChar(ref _dc, 'ú', "u");
            FillDictChar(ref _dc, '#', "");
            FillDictChar(ref _dc, '£', "L");
            FillDictChar(ref _dc, 'º', "o");

            FillDictChar(ref _dc, 'ç', "c");
            FillDictChar(ref _dc, '≈', "~");
            FillDictChar(ref _dc, '¼', "1/4");
            FillDictChar(ref _dc, '⅛', "1/8");
            FillDictChar(ref _dc, '⅜', "3/8");
            FillDictChar(ref _dc, '⅝', "5/8");
            FillDictChar(ref _dc, '⅞', "7/8");
            FillDictChar(ref _dc, '⁸', "8");
            FillDictChar(ref _dc, '₈', "8");
            FillDictChar(ref _dc, '¾', "3/4");
            FillDictChar(ref _dc, '½', "1/2");
            FillDictChar(ref _dc, '≥', "greater or equal");
            FillDictChar(ref _dc, '≤', "less or equal");
            FillDictChar(ref _dc, '¢', "cent");
            FillDictChar(ref _dc, 'π', "Pi");
            FillDictChar(ref _dc, '✓', "cm");
            FillDictChar(ref _dc, '√', "cm");
            FillDictChar(ref _dc, 'ü', "u");
            FillDictChar(ref _dc, 'έ', "e");
            FillDictChar(ref _dc, 'α', "a");
            FillDictChar(ref _dc, 'Δ', "delta");
            FillDictChar(ref _dc, 'η', "eta");
            FillDictChar(ref _dc, 'μ', "muo");
            FillDictChar(ref _dc, 'ο', "o");
            FillDictChar(ref _dc, 'κ', "k");
            FillDictChar(ref _dc, 'ρ', "ro");
            FillDictChar(ref _dc, 'τ', "tau");
            FillDictChar(ref _dc, 'ί', "iota");
            FillDictChar(ref _dc, 'δ', "delta");
            FillDictChar(ref _dc, 'º', "o");

            FillDictChar(ref _dc, '"', @"""");
            FillDictChar(ref _dc, '⸮', @"?");
            FillDictChar(ref _dc, '-', @"-");
            FillDictChar(ref _dc, '"', @"""");
            FillDictChar(ref _dc, 'φ', @"phi");
            FillDictChar(ref _dc, 'Θ', @"theta");
            FillDictChar(ref _dc, '+', @"+");
            FillDictChar(ref _dc, '°', @"o");
            FillDictChar(ref _dc, 'å', @"a");
            FillDictChar(ref _dc, '²', @"2");
            FillDictChar(ref _dc, '`', @".");
            FillDictChar(ref _dc, '+', @"+");
            FillDictChar(ref _dc, 'z', @"z");
            FillDictChar(ref _dc, '÷', @"/");
            FillDictChar(ref _dc, 'e', @"e");
            FillDictChar(ref _dc, 'Æ', @"e");            
            FillDictChar(ref _dc, 'æ', @"e");
            FillDictChar(ref _dc, 'o', @"o");
            FillDictChar(ref _dc, 'g', @"g");
            FillDictChar(ref _dc, '·', @".");
            FillDictChar(ref _dc, '4', @"4");
            FillDictChar(ref _dc, 'A', @"A");
            FillDictChar(ref _dc, 'ε', @"epsilon");
            FillDictChar(ref _dc, '2', @"2");
            FillDictChar(ref _dc, 'S', @"S");
            FillDictChar(ref _dc, 'Ω', @"omega");
            FillDictChar(ref _dc, 'Σ', @"sigma");
            FillDictChar(ref _dc, 'σ', @"sigma");
            FillDictChar(ref _dc, 'ς', @"sigma");
            FillDictChar(ref _dc, 'l', @"l");
            FillDictChar(ref _dc, '.', @".");
            FillDictChar(ref _dc, '_', @"_");
            FillDictChar(ref _dc, 'C', @"C");
            FillDictChar(ref _dc, '~', @"~"); 
            FillDictChar(ref _dc, '¬', @"not");
            FillDictChar(ref _dc, 'µ', @"mu");
            FillDictChar(ref _dc, '/', @"/");
            FillDictChar(ref _dc, 'u', @"u");
            FillDictChar(ref _dc, 'd', @"d");
            FillDictChar(ref _dc, '¥', @"Yen");
            FillDictChar(ref _dc, 'a', @"a");
            FillDictChar(ref _dc, 'Z', @"Z");
            FillDictChar(ref _dc, 'ß', @"ss");
            FillDictChar(ref _dc, '1', @"1");
            FillDictChar(ref _dc, 'ë', @"e");
            FillDictChar(ref _dc, '3', @"3");
            FillDictChar(ref _dc, 'É', @"E");
            FillDictChar(ref _dc, '«', @"""");
            FillDictChar(ref _dc, '»', @"""");
            FillDictChar(ref _dc, 'ò', @"o");
            FillDictChar(ref _dc, 'i', @"i");
            FillDictChar(ref _dc, 'D', @"D");
            FillDictChar(ref _dc, 'û', @"u");
            FillDictChar(ref _dc, 'c', @"c");
            FillDictChar(ref _dc, 'ƒ', @"f");
            FillDictChar(ref _dc, 'O', @"O");
            FillDictChar(ref _dc, '├', @"impl.");
            FillDictChar(ref _dc, '>', @">");
            FillDictChar(ref _dc, '<', @"<");
            FillDictChar(ref _dc, '"', @"");
            FillDictChar(ref _dc, 'T', @"T");
            FillDictChar(ref _dc, '5', @"5");
            FillDictChar(ref _dc, '╪', @"+=");
            FillDictChar(ref _dc, 'I', @"I");
            FillDictChar(ref _dc, 'º', @"o");
            FillDictChar(ref _dc, 'h', @"h");
            FillDictChar(ref _dc, 's', @"s");
            FillDictChar(ref _dc, 'R', @"R");
            FillDictChar(ref _dc, '0', @"0");
            FillDictChar(ref _dc, 'L', @"L");
            FillDictChar(ref _dc, 'Ö', @"O");
            FillDictChar(ref _dc, '±', @"+/=");
            FillDictChar(ref _dc, '^', @"^");
            FillDictChar(ref _dc, 'Ü', @"U");
            FillDictChar(ref _dc, ',', @",");
            FillDictChar(ref _dc, 'U', @"U");
            FillDictChar(ref _dc, '─', @"-");
            FillDictChar(ref _dc, '6', @"6");
            FillDictChar(ref _dc, '°', @"deg.");
            FillDictChar(ref _dc, 'U', @"U");
            FillDictChar(ref _dc, 'Ü', @"U");
            FillDictChar(ref _dc, '-', @"-");
            FillDictChar(ref _dc, '·', @".");

            //FillDictChar(ref _dc, '"', @"""");

            // 

            return _dc;
        }

        static Regex rexParasiticChars = new Regex(@"[{}\|;,\.:/\-_\+\.\^]", RegexOptions.Compiled),
            rexOtherParasiticChars = new Regex(@"[\[\]]", RegexOptions.Compiled),
            rexStartConcept = new(@"[#]{1,} ", RegexOptions.Compiled),
            rexDoubleSpace = new(@"[ \s]+", RegexOptions.Compiled),
            rexSYNO = new(@"[;](xSYN|xANT|xHPR|xHPO|xHOL|xMER|xMET|xCOT|xCRD|xTRP|xCOL|xTMP|CATEG)", RegexOptions.Compiled),
            //rexAllLexClasses = new(@"[\-](Noun|Proper noun|Adjective|Adverb|Preposition|Conjunction)", RegexOptions.Compiled),
            rexPartialLexClasses = new(@"([\-])((Nou|Prop|Adje|Ver|Adv|Conj|Prepo|Arti|Deter|Phr|Lett|Symb|Pref|Coll|Num|Pron|Inter|Contr|Term|Prov|Punct|Liga)[^;]+)([;])", RegexOptions.Compiled);


        static Dictionary<char, string> _dictChar = FillChars();

        static string GetCleanedUpSentenceFromUnwanted(string line, string term, string replacement)
        {
            return line.Replace(term, replacement);
        }

        //static string GetCleanRegexedLine(string line, string pattern, string replacement)
        //{
        //    return Regex.Replace(line, pattern, replacement);
        //}
        //     def cleanUpSentence(line, apattern, replacement):
        //line = re.sub(apattern, replacement, line)   
        //return line
        /// <summary>
        /// from GIL.sln
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        static string NewPurgeList(string line)
        {          
            int k;
            string theString;
            bool isAsciiChar;
            char c;

            line = line.Replace("{{trans-top", "");
            line = rexStartConcept.Replace(line, "").Trim();
            line = rexPartialLexClasses.Replace(line, " ");
            line = rexSYNO.Replace(line, " ");

            line = GetCleanedUpSentenceFromUnwanted(line, "{countable", " ");
            line = GetCleanedUpSentenceFromUnwanted(line, "|uncountable", " ");
            line = GetCleanedUpSentenceFromUnwanted(line, "|countable", " ");
            line = GetCleanedUpSentenceFromUnwanted(line, "|informal", "");
            line = GetCleanedUpSentenceFromUnwanted(line, "{informal", "");

            line = GetCleanedUpSentenceFromUnwanted(line, "{{transitive}}", "");
            line = GetCleanedUpSentenceFromUnwanted(line, "{{intransitive}}", "");
            line = GetCleanedUpSentenceFromUnwanted(line, "(now usually intransitive, with preposition or locative adverb)", "");
            line = GetCleanedUpSentenceFromUnwanted(line, "{{of a usually transitive verb}}", "");

            line = rexOtherParasiticChars.Replace(line, "");
            line = rexParasiticChars.Replace(line, " ");
            line = line.Replace("  ", " ");
            line = rexDoubleSpace.Replace(line, " ").Trim();
            char[] _chars = line.ToCharArray();
            StringBuilder sb = new StringBuilder();

            for (k = 0; k < _chars.Length; k++) // check individual character
            {
                c = _chars[k];

                byte[] utf8Bytes = Encoding.UTF8.GetBytes(new char[] { c });
                int utf8BytesLength = utf8Bytes.Length;
                isAsciiChar = utf8BytesLength == 1; // is it ascii?

                //byte firstByte = utf8Bytes[0];
                //string thestring = firstByte.ToString();
                //string binary = Convert.ToString(firstByte, 2);

                if (isAsciiChar)
                {
                    sb.Append(c);
                    continue;
                }

                if (_dictChar.ContainsKey(c))
                    theString = _dictChar[c];
                else
                    theString = " ";

                sb.Append(theString);
            }


            //line = MakeUtf8String(line);

            return sb.ToString().Trim();
        }


        //static string MakeUtf8String(string line)
        //{
        //    byte[] utf8Bytes = Encoding.UTF8.GetBytes(line);

        //    return Encoding.UTF8.GetString(utf8Bytes);
        //}

        static Regex rexLexClass = new Regex(@"[-][N]", RegexOptions.Compiled);

        static string GetTruncated(string txt, int length)
        {
            if (txt.Length >= length)
                txt = txt.Substring(0, length);

            return txt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_sentences"></param>
        /// <param name="_originalSentences"></param>
        /// <returns></returns>
        static List<string> GetListOfSentencePairs(List<string> _sentences, out List<string> _originalSentences, out string word)
        {
            Match match;

            _originalSentences = new();

            word = "";

            List<string> _tmp = new();

            if ((match = rexLexClass.Match(_sentences[0])).Success)
                word = _sentences[0].Substring(0, match.Index);

            for (int i = 0; i < _sentences.Count - 1; i++) //  - 1
            {
                for (int j = i + 1; j < _sentences.Count; j++)
                {
                    //_tmp = new List<string>();

                    //if (line.IndexOf('á') > -1)
                    //    line = line;
                    //if (_sentences[i].IndexOf("Isabela Island") > -1)
                    //    _sentences[i] = _sentences[i];

                    _originalSentences.Add(_sentences[i] + "$$$" + _sentences[j]);
                    _tmp.Add(NewPurgeList(_sentences[i]) + "$$$" + NewPurgeList(_sentences[j]));
                }
            }

            return new List<string>(_tmp);
        }

        //static IEnumerable<string> GetSentencePairs(List<string> sentences)
        //{
        //    for (int i = 0; i < sentences.Count - 1; i++)
        //    {
        //        for (int j = i + 1; j < sentences.Count; j++)
        //            yield return $"{sentences[i]}$$${sentences[j]}";
        //        //yield return $"{sentences[i]} {sentences[i + 1]}";
        //    }
        //}

        static List<List<string>> FixEntries(List<List<string>> _llists)
        {
            int i = 0, j = 0, k = 0;
            string line,
                theString;

            char c = ' ';

            int counter = 0;

            bool isAsciiChar = false;

            for (i = 0; i < _llists.Count; i++)
            {
                for (j = 0; j < _llists[i].Count; j++)
                {
                    line = _llists[i][j];

                    //++counter;

                    //if (counter == 100)
                    //    return;

                    char[] _chars = line.ToCharArray();
                    StringBuilder sb = new StringBuilder();

                    for (k = 0; k < _chars.Length; k++) // check individual character
                    {
                        c = _chars[k];

                        byte[] utf8Bytes = Encoding.UTF8.GetBytes(new char[] { c });
                        int utf8BytesLength = utf8Bytes.Length;
                        isAsciiChar = utf8BytesLength == 1; // is it ascii?

                        //byte firstByte = utf8Bytes[0];
                        //string thestring = firstByte.ToString();
                        //string binary = Convert.ToString(firstByte, 2);

                        if (isAsciiChar)
                        {
                            sb.Append(c);
                            continue;
                        }

                        if (_dictChar.ContainsKey(c))
                            theString = _dictChar[c];
                        else
                            theString = " ";

                        sb.Append(theString);
                    }

                    //if (!isAsciiChar)
                    //    Console.WriteLine(c + " --- " + line);

                    _llists[i][j] = sb.ToString();
                }


            }

            return new(_llists);
        }
        //GetTruncated

        /// <summary>
        /// MISSING_SIM_READY_FOR PROCESS
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //GetTotalWords();
            //return;

            GetProcessedWords();
            return;

            //CompareProcessedAndMaybeMissing();
            //return;

            // test utf8
            char c = '~'; // ~

            TimeSpan ts;

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(new char[] { c });

            int utf8BytesLength = utf8Bytes.Length;

            byte firstByte = utf8Bytes[0];

            string thestring = firstByte.ToString();

            string binary = Convert.ToString(firstByte, 2);


            c = '你'; // Mandarin char length = 3 bytes.

            utf8Bytes = Encoding.UTF8.GetBytes(new char[] { c });

            utf8BytesLength = utf8Bytes.Length;

            firstByte = utf8Bytes[0];

            thestring = firstByte.ToString();

            binary = Convert.ToString(firstByte, 2);

            c = 'د'; // Arabic length = 2

            utf8Bytes = Encoding.UTF8.GetBytes(new char[] { c });

            utf8BytesLength = utf8Bytes.Length;

            firstByte = utf8Bytes[0];

            thestring = firstByte.ToString();

            binary = Convert.ToString(firstByte, 2);

            c = '선'; // Æ ≡ - ≈ ⅞ ₈ ≥ √ ✓ Δ ί 선

            thestring = "aefé你"; // your string

            int len = Encoding.UTF8.GetByteCount(thestring);

            int _THRESHOLD_ = 7000;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            Console.WriteLine(DateTime.Now.ToString("yyyyMMddHHmmss"));

            string pyFile = "compareNew.py";

            string path = "",

                word = "";

            bool file_exists = File.Exists(pyFile);
            if (!file_exists)
            {
                path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\";
            }

            pyFile = path + pyFile;
            file_exists = File.Exists(pyFile);
            if (!file_exists)
            {
                Console.WriteLine("Python File missing: " + pyFile);
                return;
            }

            // NEW 01/31/2024
            path = _DIRPREFIX + @"\DATA\";

            // from GIL.sln
            //ForAISimilarityAndClassification_.txt 
            string fileIn = path + fileToStart + ".txt";
            //string fileIn = fileToStart + ".txt";

            file_exists = File.Exists(fileIn);
            if (!file_exists)
            {
                Console.WriteLine("File missing: " + fileIn);
                return;
            }

            // testing entries

            List<List<string>> aLexClassEntries = GetListOfEntries(fileIn),
               aLexClassEntriesForUse = new();

            //aLexClassEntriesForUse = FixEntries(aLexClassEntries);

            List<string> _sentences = new List<string>(),
                _work = new List<string>(),
                _lexOut = new();

            // Invoke the Python function using Process
            Process process = new Process();
            
            /*
            Arguments = "C:\\myPyscripts\\translateEng.py --text \""+inputText + "\\" ,
            */
            // PROCESS INVOKES python.exe to execute the python code (CompareSentences.py) 

            process.StartInfo = new ProcessStartInfo("python", pyFile)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true, // hide console window 
            };
            process.Start();

            double similarity;

            //List<double> _similarities = new List<double>();

            List<string> _originalSentences = new();
            List<string> _ses = new List<string>();

            int counter = 0;

            string fileOut;


            // Establish communication channels
            using (StreamWriter writer = process.StandardInput)
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    //for (int i = 0; i < aLexClassEntries.Count; i++)
                    for (int i = 0; (i < _THRESHOLD_ && i < aLexClassEntries.Count); i++)
                    {
                        counter = i;

                        if (counter % 100 == 0)
                        {
                            Console.WriteLine("counter = " + counter.ToString());
                            ts = sw.Elapsed;
                            Console.WriteLine("Elapsed: " + ts.ToString("mm\\:ss\\.ff"));
                        }

                        if (counter % 1000 == 0) // place inside above saves one test
                        {
                            fileOut = _RESULTSPREFIX + fileToStart + counter.ToString() + "_.txt";
                            Util.PrintOut(ref _lexOut, fileOut);
                            _lexOut.Clear();
                        }

                        _sentences = aLexClassEntries[i]; // all sentences to compare
                        //word = "";

                        _work = GetListOfSentencePairs(_sentences, out _originalSentences, out word);

                        //
                        //if (word.StartsWith("epidot"))
                        //    word = word;

                        for (int sim = 0; sim < _work.Count; sim++)
                        {
                            string pair = _work[sim];
                            _ses.Clear();
                            _ses.Add(pair.Split("$$$")[0]);
                            _ses.Add(pair.Split("$$$")[1]);

                            if(IsSpecial(_ses))
                            {
                                _originalSentences[sim] += "SIM#" + ((decimal)0.0).ToString();
                                _lexOut.Add(_originalSentences[sim]);
                                continue;
                            }

                            if (IsMineral(_ses))
                            {
                                _originalSentences[sim] += "SIM#" + ((decimal)1.0).ToString();
                                _lexOut.Add(_originalSentences[sim]);
                                continue;
                            }


                            if (!process.HasExited)
                            {
                                writer.WriteLine(pair);
                                writer.Flush(); // Ensure data is sent immediately

                                string similarityString = reader.ReadLine(); // Read similarity

                                try
                                {
                                    similarity = double.Parse(similarityString);
                                    _originalSentences[sim] += "SIM#" + ((decimal)similarity).ToString();
                                    _lexOut.Add(_originalSentences[sim]);
                                }
                                catch (Exception ex) 
                                {
                                    Console.WriteLine("FAILED: " 
                                        + _originalSentences[0] + System.Environment.NewLine
                                        + _originalSentences[1]);

                                    fileOut = _RESULTSPREFIX + fileToStart + counter.ToString() + "_.txt";
                                    Util.PrintOut(ref _lexOut, fileOut);
                                    return ;
                                }
                                
                                
                                //Console.WriteLine($"1-Similarity between {_ses[0]} and {_ses[1]}: {similarity}");
                            }
                            //else
                            //{
                            //    writer.WriteLine(pair);
                            //    writer.Flush(); // Ensure data is sent immediately

                            //    // Read the similarity result
                            //    string similarityString = reader.ReadLine();
                            //    similarity = double.Parse(similarityString);
                            //    _originalSentences[sim] += "SIM#" + ((decimal)similarity).ToString();
                            //    _lexOut.AddRange(_originalSentences);

                            //    //Console.WriteLine($"2-Similarity between {_ses[0]} and {_ses[1]}: {similarity}");
                            //}
                        }
                    }
                    // Read the output from the Python script
                    //string output = process.StandardOutput.ReadToEnd();
                    //similarity = double.Parse(output);

                    //Console.WriteLine("Similarity: {0}", similarity);

                    process.Close();
                }
            }

            // starts from ^tick
            fileOut = _RESULTSPREFIX + _THRESHOLD_.ToString() + "_" + quant + "_.txt";
            Util.PrintOut(ref _lexOut, fileOut);

            sw.Stop();
            ts = sw.Elapsed;

            Console.WriteLine("Counter: {0}", counter);

            Console.WriteLine("Elapsed: " + ts.ToString("mm\\:ss\\.ff"));

            Console.Read();
        }

        static Regex rexMineral = new Regex(@"Noun ([A-Z][a-z]){2}", RegexOptions.Compiled);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ses"></param>
        /// <returns></returns>
        static bool IsSpecial(List<string> _ses)
        {
            if (
            _ses[0].IndexOf("initialism of") > -1
            || _ses[1].IndexOf("initialism of") > -1
            ) return true;


            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ses"></param>
        /// <returns></returns>
        static bool IsMineral(List<string> _ses)
        {
            if (
            _ses[0].IndexOf("[[mineral]]") > -1
            && _ses[1].IndexOf("mineral") > -1
            ) return true;

            if (
                _ses[0].IndexOf("[[mineral]]") > -1
                && _ses[1].IndexOf(" mineral") > -1
                ) return true;

            if (_ses[0].IndexOf("mineral") > -1
                && _ses[1].IndexOf("inorganic") > -1
                ) return true;

            if (_ses[0].IndexOf("mineral") > -1
                && _ses[1].IndexOf("{{chemistry") > -1
                ) return true;


            if (_ses[0].IndexOf("mineral") > -1
                && _ses[1].IndexOf("in the plural") > -1
                ) return true;



            return false;
        }

        static Dictionary<string, Dictionary<string, List<string>>> GetDictionaryFromKList(List<string> _lines)
        {
            var output = _lines
                .Select(line => (First: line.Split('╧')[0], Second: line.Substring(line.IndexOf('╧') + 1, line.IndexOf('╤') - line.IndexOf('╧') - 1), Suffix: line))
                .GroupBy(x => x.First)
                .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Second).ToDictionary(x => x.Key, x => x.Select(y => y.Suffix).ToList()));
            
            return output;
        }

    }
}


/* List of List<string>
var output = _lines
    .Select(line => new { Line = line, Pos = line.IndexOf('╤') })
    //.Select(x => new { Prefix = x.Line.Substring(0, x.Pos), Suffix = x.Line.Substring(x.Pos + 1) })
    .Select(x => new { Prefix = x.Line.Substring(0, x.Pos), Suffix = x.Line })
    .GroupBy(x => x.Prefix)
    .Select(g => g.Aggregate(new List<string>(), (list, x) =>
    {
        list.Add(x.Suffix);
        return list;
    }))
// Convert the result to a list of list<string>
.ToList();
*/
