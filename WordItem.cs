
namespace SentenceRecovery
{
    class WordItem
    {
        public string Word { get; }

        public double Weight { get; }

        public float CapitalRatio { get; }

        public bool PreferCapitalized { get; }

        public WordItem(string word, int frequency, int frequencyLower, double weight)
        {
            Word = word.ToLower();
            Weight = weight;
            CapitalRatio = frequency == 0 ? 0 : 1 - (float)frequencyLower / frequency;
            PreferCapitalized = CapitalRatio > 0.7f;
        }

        public string GetPreferredTypeWord()
        {
            return PreferCapitalized ? Capitalize(Word) : Word;
        }

        private static string Capitalize(string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }
    }
}
