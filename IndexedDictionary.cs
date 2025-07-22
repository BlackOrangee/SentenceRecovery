
namespace SentenceRecovery
{
    class IndexedDictionary
    {
        private readonly Dictionary<(int, string), List<WordItem>> indexedFrequencyDict = new Dictionary<(int, string), List<WordItem>>();

        public IndexedDictionary(List<string> dictionary)
        {
            foreach (string line in dictionary)
            {
                string[] parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                if (parts.Length > 1 && !int.TryParse(parts[1], out _))
                {
                    continue;
                }

                string word = parts[0];
                int frequency = parts.Length > 1 && int.TryParse(parts[1], out int freq) ? freq : 0;
                int frequencyLower = parts.Length > 2 && int.TryParse(parts[3], out int freqLower) ? freqLower : 0;
                double weight = parts.Length > 6 && double.TryParse(parts[6], out double w) ? w : 0.0;

                string sorted = String.Concat(word.ToLower().OrderBy(c => c));
                int length = word.Length;
                var key = (length, sorted);


                if (!indexedFrequencyDict.TryGetValue(key, out var list))
                {
                    list = new List<WordItem>();
                    indexedFrequencyDict[key] = list;
                }

                list.Add(new WordItem(word, frequency, frequencyLower, weight));
            }
        }

        public List<WordItem> FindMatches(string serchableWord, double minWeight)
        {
            serchableWord = serchableWord.ToLower();
            int length = serchableWord.Length;

            if (!serchableWord.Contains('*'))
            {
                var key = (length, String.Concat(serchableWord.ToLower().OrderBy(c => c)));

                if (indexedFrequencyDict.TryGetValue(key, out var list))
                {
                    List<WordItem> exact = list
                        .Where(m => m.Weight > minWeight)
                        .OrderByDescending(m => m.Weight)
                        .ToList();

                    if (exact.Count > 0)
                    {
                        return exact;
                    }
                }

                return new();
            }

            string sortedSerchableWord = String.Concat(serchableWord
                                                    .Where(c => c != '*')
                                                    .OrderBy(c => c));

            var matches = new List<WordItem>();

            foreach (var kvp in indexedFrequencyDict)
            {
                var (keyLen, keySorted) = kvp.Key;
                if (keyLen != length)
                {
                    continue;
                }

                if (!ContainsAllLettersSimple(keySorted, sortedSerchableWord))
                {
                    continue;
                }

                foreach (WordItem wordItem in kvp.Value)
                {
                    if (MatchesPattern(serchableWord, wordItem.Word))
                    {
                        matches.Add(wordItem);
                    }
                }
            }

            return matches
                .Where(m => m.Weight > minWeight)
                .OrderByDescending(m => m.Weight)
                .ToList();
        }

        private static bool MatchesPattern(string pattern, string word)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] != '*' && pattern[i] != word[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsAllLettersSimple(string source, string required)
        {
            var chars = source.ToList();

            foreach (char c in required)
            {
                if (!chars.Remove(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
