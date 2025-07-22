namespace SentenceRecovery
{
    class SmartSegmentedRestorer
    {
        private readonly TextRestorer restorer;


        public SmartSegmentedRestorer(IndexedDictionary dictionary, NGramModel bigramModel)
        {
            restorer = new TextRestorer(dictionary, bigramModel, Mode.UltraLenient);
        }
        public List<string> RestoreAdaptively(string input)
        {
            List<RecoveryRowItem> adaptivelyResult = RestoreAdaptivelyRecursive(input);

            List<RecoveryRowItem> refined = RefineJunctionsBetweenChunks(adaptivelyResult);


            List<string> result = new List<string>();

            result.Add(string.Join(" ", adaptivelyResult.Select(r => r.Sentence)));
            result.Add(string.Join(" ", refined.Select(r => r.Sentence)));
           
            return result;
        }

        private List<RecoveryRowItem> RestoreAdaptivelyRecursive(string input, HashSet<string> visitedChunks = null)
        {
            visitedChunks ??= new HashSet<string>();
            List<RecoveryRowItem> previewRows = PreviewRowsAnalize(input, visitedChunks);

            List <int> boundaries = GetChunkBoundaries(input, previewRows);

            List<string> chunks = SplitIntoChunks(input, boundaries);

            var results = new List<RecoveryRowItem>();

            foreach (var chunk in chunks)
            {
                if (visitedChunks.Contains(chunk))
                {
                    continue;
                }
                else if (chunk.Length > 30)
                {
                    visitedChunks.Add(chunk);
                    var recursive = RestoreAdaptivelyRecursive(chunk, visitedChunks);
                    results.AddRange(recursive);
                }
                else
                {
                    results.Add(RestoreChunkAdaptively(chunk));
                }
            }
            return results;
        }

        private RecoveryRowItem RestoreChunkAdaptively(string chunk)
        {
            RecoveryRowItem best = new RecoveryRowItem(chunk, double.MinValue, chunk);

            foreach (var mode in Enum.GetValues<Mode>())
            {
                var candidates = restorer.Restore(chunk, mode);

                if (candidates.Count > 0 && candidates[0].Score > best.Score)
                {
                    best.Sentence = candidates[0].Sentence;
                    best.Score = candidates[0].Score;
                }

            }

            return best;
        }

        private static bool ShouldSkipMode(Mode mode, int visitedCount)
        {
            return visitedCount < 3 && (mode == Mode.UltraStrict || mode == Mode.Strict || mode == Mode.Soft);
        }

        private List<RecoveryRowItem> PreviewRowsAnalize(string input, HashSet<string> visitedChunks)
        {
            List<RecoveryRowItem> previewRows = new List<RecoveryRowItem>();
            foreach (var mode in Enum.GetValues<Mode>())
            {
                if (ShouldSkipMode(mode, visitedChunks.Count))
                {
                    continue;
                }

                var rows = restorer.Restore(input, mode, 20);
                if (rows.Count > 0)
                {
                    previewRows.AddRange(rows);

                    break;
                }
            }

            return previewRows;
        }

        private static List<int> GetChunkBoundaries(string input, List<RecoveryRowItem> previewRows)
        {
            var wordsPositionsWeight = BoundaryDetector.AlalizeRows(previewRows);

            List<int> boundaries = new List<int>();
            for (int i = input.Length / 2; i < input.Length; i += input.Length / 2)
            {
                boundaries.Add(BoundaryDetector.FindBestWordEndNear(wordsPositionsWeight, i));
            }

            return boundaries.Distinct().OrderBy(b => b).ToList();
        }

        private static List<string> SplitIntoChunks(string input, List<int> boundaries)
        {
            List<string> chunks = new List<string>();
            int start = 0;
            foreach (var b in boundaries)
            {
                if (b > start && b <= input.Length)
                {
                    chunks.Add(input.Substring(start, b - start));
                    start = b;
                }
            }

            if (start < input.Length)
            {
                chunks.Add(input.Substring(start));
            }

            return chunks;
        }

        private static List<string> GetWords(string sentence)
        {
            return sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private RecoveryRowItem GetBestMerge(string overlap)
        {
            var bestMerge = new RecoveryRowItem(overlap, double.MinValue);
            int bestWordCount = int.MaxValue;

            foreach (var mode in Enum.GetValues<Mode>())
            {
                var candidates = restorer.Restore(overlap, mode);
                if (candidates.Count == 0) continue;

                var candidate = candidates
                    .OrderBy(c => c.Sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                    .ThenByDescending(c => c.Score)
                    .First();

                int candidateWords = candidate.Sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                if ((candidateWords < bestWordCount))
                {
                    bestMerge = new RecoveryRowItem(candidate.Sentence, candidate.Score, overlap);
                    bestWordCount = candidateWords;
                }
            }

            return bestMerge;
        }

        private List<RecoveryRowItem> RefineJunctionsBetweenChunks(List<RecoveryRowItem> results)
        {
            var refined = new List<RecoveryRowItem>();

            for (int i = 0; i < results.Count - 1; i++)
            {
                RecoveryRowItem left = results[i];
                RecoveryRowItem right = results[i + 1];

                List<string> leftWords = GetWords(left.Sentence);
                List<string> rightWords = GetWords(right.Sentence);

                if (leftWords.Count < 2 || rightWords.Count < 2)
                {
                    refined.Add(left);
                    continue;
                }

                int cutFromLeft = string.Join("", leftWords.Skip(leftWords.Count - 2)).Length;
                int cutFromRight = string.Join("", rightWords.Take(2)).Length;

                string overlap = string.Concat(
                    left.Original.Substring(left.Original.Length - cutFromLeft, cutFromLeft),
                    right.Original.Substring(0, cutFromRight)
                );

                RecoveryRowItem bestMerge = GetBestMerge(overlap);

                var mergedWords = bestMerge.Sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int mid = mergedWords.Length / 2;

                var newLeftSentence = string.Join(" ", leftWords.Take(leftWords.Count - 2).Concat(mergedWords.Take(mid)));
                var newRightSentence = string.Join(" ", mergedWords.Skip(mid).Concat(rightWords.Skip(2)));

                refined.Add(new RecoveryRowItem(newLeftSentence, left.Score, left.Original));
                if (i == results.Count - 2)
                {
                    refined.Add(new RecoveryRowItem(newRightSentence, right.Score, right.Original));
                }
                else
                {
                    results[i + 1] = new RecoveryRowItem(newRightSentence, right.Score, right.Original);
                }
            }

            if (results.Count > 0)
            {
                var last = results[^1];
                if (!refined.Any(r => r.Original == last.Original))
                {
                    refined.Add(last);
                }
            }

            return refined;
        }
    }
}
