namespace SentenceRecovery
{
    internal class NGramModel
    {
        private readonly Dictionary<string, double> weights = new();

        public NGramModel(List<string> nGrams)
        {
            Dictionary<string, int> frequencies = new();

            foreach (var line in nGrams)
            {
                string[] parts = line.Split([','], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    continue;
                }

                string ngram = parts[0].ToLower();

                if (!int.TryParse(parts[1], out int freq) || freq <= 0)
                {
                    continue;
                }

                frequencies[ngram] = freq;
            }

            long totalFreq = frequencies.Values.Select(v => (long)v).Sum();


            foreach (var (ngram, freq) in frequencies)
            {
                double lg10wf = Math.Log10((double)freq / totalFreq * 1_000_000);
                weights[ngram] = Math.Round(lg10wf * 10000) / 10000;
            }
        }

        public double GetWeight(string ngram)
        {
            return weights.TryGetValue(ngram, out double weight) ? weight : 0;
        }
    }
}
