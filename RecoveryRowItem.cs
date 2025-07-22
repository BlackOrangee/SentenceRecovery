
namespace SentenceRecovery
{
    public class RecoveryRowItem
    {
        public string Original { get; set; }
        public string Sentence { get; set; }
        public double Score { get; set; }

        public RecoveryRowItem(string sentence, double score)
        {
            Sentence = sentence;
            Score = score;
            Original = "";
        }

        public RecoveryRowItem(string sentence, double score, string original)
        {
            Original = original;
            Sentence = sentence;
            Score = score;
        }
    }
}
