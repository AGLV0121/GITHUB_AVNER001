/*
This code shows: 
1 / how to invoke a python function from a C# process.
It is especially valuable in case of long iterative process,
to get the speed of C# and functionality of python.
2/ how to store a large language model locally.
*/

using System;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace ConsoleAppSIMILARITY
{
    public partial class Program
    {
            //###
        static string quant = "1"; 
        static string fileToStart = @"SimilarityAndClassification_" + quant+ "_"; // Test_1_

        
        //GetTruncated
        static string _DIRPREFIX = @"F:\____MODELS\";
        static string _RESULTSPREFIX = _DIRPREFIX + @"RESULTS\";
        static string _INBASEDIR = _DIRPREFIX + @"\DATA\";

        static Regex rexLexEx = new Regex(@"([\-])((Nou|Prop|Adje|Ver|Adv|Conj|Prepo|Arti|Deter|Phr|Lett|Symb|Pref|Coll|Num|Pron|Inter|Contr|Term|Prov|Punct|Liga).*)([;])", RegexOptions.Compiled);
        //static Regex rexLex =      new Regex(@"[\-](Nou|Prop|Adje|Ver|Adv|Conj|Prepo|Arti|Deter|Phr|Lett|Symb|Pref|Coll|Num|Pron|Inter|Contr|Term|Prov|Punct|Liga)", RegexOptions.Compiled);


        /// <summary>
        /// MISSING_SIM_READY_FOR PROCESS
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
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

            // To monitor process and save every _THRESHOLD_ input lines processed. 
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

            /* Replace this with your file to process.
             * Here, two strings that must be evaluatd for similarity
             * are on the same line, separated by $$$
             */
            
            path = _DIRPREFIX + @"\DATA\"; // NEW 01/31/2024

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

            List<List<string>> aLexClassEntries = GetListOfEntries(fileIn);

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
                            PrintOut(_lexOut, fileOut);
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
                                    PrintOut(_lexOut, fileOut);
                                    return ;
                                }
                                
                                
                                //Console.WriteLine($"1-Similarity between {_ses[0]} and {_ses[1]}: {similarity}");
                            }
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
            PrintOut(_lexOut, fileOut);

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

        static Dictionary<string, Dictionary<string, List<string>>> GetDictionaryFromKList(List<string> _lines)
        {
            var output = _lines
                .Select(line => (First: line.Split('╧')[0], Second: line.Substring(line.IndexOf('╧') + 1, line.IndexOf('╤') - line.IndexOf('╧') - 1), Suffix: line))
                .GroupBy(x => x.First)
                .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Second).ToDictionary(x => x.Key, x => x.Select(y => y.Suffix).ToList()));
            
            return output;
        }



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



    }
}
