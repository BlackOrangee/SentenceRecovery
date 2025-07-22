using SentenceRecovery;

class Program
{
    static void Main(string[] args)
    {
        string sourceSentenceFileName = "source.txt";
        string dictionaryDirectoryName = "dictionaries";
        string dictionaryFileName = "SUBTLEXus74286wordstextversion.txt";

        List<string> sourceSentence = FileWorker.ReadTextFromFile(sourceSentenceFileName);

        List<string> dictionary = FileWorker.ReadTextFromFile($"{dictionaryDirectoryName}/{dictionaryFileName}");

        List<string> bigrams = FileWorker.ReadTextFromFile($"{dictionaryDirectoryName}/2grams_english-fiction.csv");

        IndexedDictionary indexedDictionary = new IndexedDictionary(dictionary);
        NGramModel bigramModel = new NGramModel(bigrams);


        SmartSegmentedRestorer smartSegmentedRestorer = new SmartSegmentedRestorer(indexedDictionary, bigramModel);

        List<string> results = smartSegmentedRestorer.RestoreAdaptively(sourceSentence.First());

        Console.WriteLine($"Restored sentences:             { results[0]}");
        Console.WriteLine($"Alternative Restored sentences: {results[1]}");

        FileWorker.WriteTextToFile("result.txt", results);

        Console.ReadLine();
    }
}