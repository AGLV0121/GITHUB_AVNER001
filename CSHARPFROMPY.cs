using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SentenceTransformers;
using Torch;

public class Program
{
    private static string REPO_DIR = "f:/____MODELS/DATA/";
    private static string MODEL_DIR = "f:/____MODELS/";
    private static string OUTPUT_DIR = "f:/____MODELS/RESULTS/";

    private static List<string> _output_list = new List<string>();

    private static SentenceTransformer mpnetSentenceComparerInit()
    {
        string modelSaveName = "all-mpnet-base-v2.pkl";
        string modelName = "all-mpnet-base-v2";
        string fileSave = Path.Combine(MODEL_DIR, modelSaveName);

        SentenceTransformer model;

        if (File.Exists(fileSave))
        {
            model = read_model(fileSave);
        }
        else
        {
            model = new SentenceTransformer(modelName);
            save_model(model, fileSave);
        }

        return model;
    }

    private static void save_model(SentenceTransformer model, string file_path)
    {
        model.Save(file_path);
    }

    private static SentenceTransformer read_model(string file_path)
    {
        return new SentenceTransformer(file_path);
    }

    private static float compute_similarity_scoresFromEmbeddings(float[] embeddings1, float[] embeddings2)
    {
        float similarity_score = cosine_similarity(embeddings1, embeddings2);
        return similarity_score;
    }

    private static float cosine_similarity(float[] embeddings1, float[] embeddings2)
    {
        float dotProduct = DotProduct(embeddings1, embeddings2);
        float magnitude1 = Magnitude(embeddings1);
        float magnitude2 = Magnitude(embeddings2);
        return dotProduct / (magnitude1 * magnitude2);
    }

    private static float DotProduct(float[] vector1, float[] vector2)
    {
        return vector1.Zip(vector2, (a, b) => a * b).Sum();
    }

    private static float Magnitude(float[] vector)
    {
        return (float)Math.Sqrt(vector.Select(x => x * x).Sum());
    }

    private static (float[], float[]) getEmbeddings(SentenceTransformer model, string sentence1, string sentence2)
    {
        float[] embeddings1 = model.Encode(sentence1);
        float[] embeddings2 = model.Encode(sentence2);
        return (embeddings1, embeddings2);
    }

    private static List<string> getListFromSPlit(string rex, string line)
    {
        return Regex.Split(line, rex).ToList();
    }

    private static List<string> getStringFromDelimiters(string del1, string del2, string line)
    {
        var match = Regex.Match(line, $"{del1}(.*){del2}");
        return match.Success ? new List<string> { match.Groups[1].Value } : new List<string>();
    }

    private static string purgeLine(List<string> _list, int index)
    {
        string line = _list[index].Trim();
        line = line.Replace("\ufeff", "");
        return line;
    }

    private static void process_list(List<string> _list, SentenceTransformer model)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            string lineI = purgeLine(_list, i);
            List<string> _elementsI = getListFromSPlit("SimilScore#[\\$]", lineI);

            string keyI = _elementsI[0].Trim();
            string similarity_sentenceI = _elementsI[1].Trim();

            for (int j = i + 1; j < _list.Count; j++)
            {
                string lineJ = purgeLine(_list, j);
                List<string> _elementsJ = getListFromSPlit("SimilScore#[\\$]", lineJ);
                string keyJ = _elementsJ[0].Trim();
                string similarity_sentenceJ = _elementsJ[1].Trim();

                (float[] embeddings1, float[] embeddings2) = getEmbeddings(model, similarity_sentenceI, similarity_sentenceJ);
                float cos_similarity = cosine_similarity(embeddings1, embeddings2);

                string outputLine = $"{cos_similarity}#^{keyI}#^{keyJ}";
                _output_list.Add(outputLine + "\r\n");
            }

            _output_list.Add("---" + "\r\n");
        }
    }

    private static void EXECUTETHIS()
    {
        bool use_cuda = Torch.Cuda.IsAvailable();
        float increment = 0;

        string similaryBase = Path.Combine(REPO_DIR, "ForAISimilarityAndClassification_1_.txt");

        string[] _alist = File.ReadAllText(similaryBase).Split(new[] { "------" }, StringSplitOptions.None);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (var item in _alist)
        {
            List<string> _localList = item.Split('\n').Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            process_list(_localList, model);
            increment++;
            if (increment == 20)
                break;
        }

        string outFile = Path.Combine(OUTPUT_DIR, "SIMILARITY_PAIRS_READATEND_WITH_CYTHON.txt");

        File.WriteAllLines(outFile, _output_list);

        stopwatch.Stop();
        Console.WriteLine($"--- {stopwatch.Elapsed.TotalSeconds} seconds ---");
    }

//    public static void Main()
//    {
//        EXECUTETHIS();
//    }
//}