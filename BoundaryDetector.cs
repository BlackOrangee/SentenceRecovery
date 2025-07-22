namespace SentenceRecovery
{
    public static class BoundaryDetector
    {
        public static Dictionary<(int, int), int> AlalizeRows(List<RecoveryRowItem> rows)
        {
            Dictionary<(int, int), int> wordsPositionsWeight = new Dictionary<(int, int), int>();

            foreach (var row in rows)
            {
                string sentence = row.Sentence;
                int indexInOriginal = 0;

                List<string> words = sentence.Split(' ').ToList();
                foreach (var word in words)
                {
                    int start = indexInOriginal;
                    int end = start + word.Length - 1;

                    var key = (start, end);

                    if (!wordsPositionsWeight.TryGetValue(key, out int count))
                    {
                        count = 0;
                    }

                    wordsPositionsWeight[key] = count + 1;

                    indexInOriginal += word.Length;
                }
            }

            return wordsPositionsWeight;
        }

        public static int FindBestWordEndNear(
            Dictionary<(int start, int end), int> wordsPositionsWeight,
            int rowLength = 40,
            int offsetRange = 5
        )
        {
            int bestWeight = int.MaxValue;
            int bestEnd = rowLength;
            int bestLength = -1;

            bool IsBetter(int weight, int length)
            {
                if (weight < bestWeight)
                {
                    return true;
                }

                if (weight == bestWeight && length > bestLength)
                {
                    return true;
                }

                return false;
            }

            foreach (var kvp in wordsPositionsWeight)
            {
                var (start, end) = kvp.Key;
                int weight = kvp.Value;
                int length = end - start;

                if (start == rowLength || end == rowLength)
                {
                    if (IsBetter(weight, length))
                    {
                        bestEnd = end;
                        bestLength = length;
                        bestWeight = weight;
                    }
                }
            }

            if (bestWeight == int.MaxValue)
            {
                for (int offset = 1; offset <= offsetRange; offset++)
                {
                    foreach (var kvp in wordsPositionsWeight)
                    {
                        var (start, end) = kvp.Key;
                        int weight = kvp.Value;
                        int length = end - start;

                        if (start == rowLength - offset || start == rowLength + offset ||
                            end == rowLength - offset || end == rowLength + offset)
                        {
                            if (IsBetter(weight, length))
                            {
                                bestEnd = end;
                                bestLength = length;
                                bestWeight = weight;
                            }
                        }
                    }
                }
            }

            return bestEnd + 1;
        }
    }
}
