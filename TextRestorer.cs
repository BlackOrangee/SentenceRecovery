namespace SentenceRecovery
{
    class TextRestorer
    {
        private readonly IndexedDictionary dictionary;
        private readonly NGramModel bigramModel;
        private readonly List<RecoveryRowItem> results = new();
        private double maxFoundScoreSoFar = 0;

        private RestorationConfig config;

        private const int MinWordLength = 1;
        private const int MaxWordLength = 15;

        public TextRestorer(IndexedDictionary dictionary, NGramModel bigramModel, Mode mode)
        {
            this.dictionary = dictionary;
            this.bigramModel = bigramModel;
            this.config = RestorationConfig.Get(mode);
        }

        public List<RecoveryRowItem> Restore(string input, Mode mode, int? limit = null, int? take = null)
        {
            maxFoundScoreSoFar = 0;
            results.Clear();
            config = RestorationConfig.Get(mode);
            RestoreRecursive(input, 0, new List<string>(), 0, 0, 0, limit);

            Console.WriteLine($"\nMode: {mode}; Results: {results.Count}");
            return results.OrderByDescending(r => r.Score).Take(take ?? int.MaxValue).ToList();
        }


        private static double WordLengthWeightConfiguration(int len)
        {
            switch (len)
            {
                case 1:
                    return 6.0;
                case 2:
                    return 4.7;
                case 3:
                    return 4.1;
                case 4:
                    return 3.5;
                case 5:
                    return 3.0;
                case 6:
                    return 1.1;
                case 7:
                    return 3.3;
                case 8:
                    return 3.4;
                case 9:
                    return 3.5;
                default:
                    return 2.2;
            }
        }

        private void RestoreRecursive(
            string input,
            int position,
            List<string> currentWords,
            double currentScore,
            int consecutiveSingleLetters = 0,
            int consecutiveShortWords = 0,
            int? limit = null
        )
        {
            if (limit != null && results.Count >= limit)
            {
                return;
            }

            if (position >= input.Length)
            {
                FinalizeResult(currentWords, currentScore);
                return;
            }

            if (consecutiveSingleLetters > config.MaxConsecutiveSingleLetters || consecutiveShortWords > config.MaxConsecutiveShortWords)
            {
                return;
            }

            if (IsPotentialTooLow(currentScore, input.Length - position))
            {
                return;
            }

            for (int len = MinWordLength; len <= MaxWordLength && position + len <= input.Length; len++)
            {
                string fragment = input.Substring(position, len);

                List<WordItem> matches = dictionary.FindMatches(fragment, WordLengthWeightConfiguration(len)).ToList();

                foreach (WordItem wordItem in matches)
                {
                    if (len == 1 && wordItem.Word != "i" && wordItem.Word != "a")
                    {
                        continue;
                    }

                    string nextWord = wordItem.Word.ToLower();
                    double bigramWeight = GetBigramWeight(currentWords, nextWord);

                    currentWords.Add(wordItem.GetPreferredTypeWord());

                    bool isSingle = nextWord.Length == 1;
                    bool isShort = nextWord.Length <= 4;

                    double penalty = CalculatePenalty(currentWords, consecutiveSingleLetters, consecutiveShortWords);

                    double score = currentScore + wordItem.Weight;
                    if (bigramWeight > 0)
                    {
                        score += bigramWeight * config.BigramWeightMultiplier;
                    }

                    score -= penalty;

                    RestoreRecursive(
                        input,
                        position + len,
                        currentWords,
                        score,
                        isSingle ? consecutiveSingleLetters + 1 : 0,
                        isShort ? consecutiveShortWords + 1 : 0,
                        limit
                    );

                    currentWords.RemoveAt(currentWords.Count - 1);
                }
            }
        }

        private bool IsPotentialTooLow(double currentScore, int remainingLength)
        {
            int estimatedWordsRemaining = (remainingLength) / 5;
            double estimatedRemainingScore = estimatedWordsRemaining * config.EstimatedScorePerWord;

            double potential = currentScore + estimatedRemainingScore * config.OptimismFactor;

            return potential < maxFoundScoreSoFar * config.PotentialThreshold;
        }

        private void FinalizeResult(List<string> currentWords, double currentScore)
        {
            currentScore *= config.ScoreMultiplier;
                string sentence = string.Join(" ", currentWords);
                results.Add(new RecoveryRowItem(sentence, Math.Round(currentScore, 2)));

                Console.WriteLine("Score: " + Math.Round(currentScore, 2) + " Sentence: " + sentence);

                if (currentScore > maxFoundScoreSoFar)
                {
                    maxFoundScoreSoFar = currentScore;
                }
        }

        private double GetBigramWeight(List<string> currentWords, string nextWord)
        {
            double bigramWeight = 0;

            if (currentWords.Count > 0)
            {
                string lastWord = currentWords.Last().ToLower();
                string bigram = $"{lastWord} {nextWord}";
                bigramWeight = bigramModel.GetWeight(bigram);
            }

            return bigramWeight;
        }

        private double CalculatePenalty(List<string> currentWords, int consecutiveSingleLetters, int consecutiveShortWords)
        {
            double penalty = 0;

            if (consecutiveSingleLetters > config.MaxConsecutiveSingleLetters)
            {
                penalty += (consecutiveSingleLetters - config.MaxConsecutiveSingleLetters) * config.SingleLetterPenalty;
            }

            if (consecutiveShortWords > config.MaxConsecutiveShortWords)
            {
                penalty += (consecutiveShortWords - config.MaxConsecutiveShortWords) * config.ShortWordPenalty;
            }

            int totalShortWords = currentWords.Count(w => w.Length <= 3);
            penalty += totalShortWords * config.ShortWordGlobalPenalty;

            return penalty;
        }
    }
}

