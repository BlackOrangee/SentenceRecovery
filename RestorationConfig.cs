namespace SentenceRecovery
{
    public enum Mode
    {
        UltraStrict,
        Strict,
        Soft,
        Lenient,
        Lenient_2,
        UltraLenient,
    }

    public class RestorationConfig
    {
        public double OptimismFactor { get; set; }
        public double EstimatedScorePerWord { get; set; }
        public double PotentialThreshold { get; set; }
        public int MaxConsecutiveSingleLetters { get; set; }
        public int MaxConsecutiveShortWords { get; set; }
        public double SingleLetterPenalty { get; set; }
        public double ShortWordPenalty { get; set; }
        public double ShortWordGlobalPenalty { get; set; }
        public double BigramWeightMultiplier { get; set; }
        public double ScoreMultiplier { get; set; }

        public static RestorationConfig Get(Mode mode)
        {

            return mode switch
            {
                Mode.UltraStrict => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 0,
                    MaxConsecutiveShortWords = 1,
                    SingleLetterPenalty = 2.0,
                    ShortWordPenalty = 1.5,
                    ShortWordGlobalPenalty = 0.5,
                    EstimatedScorePerWord = 4.0,
                    OptimismFactor = 0.6,
                    PotentialThreshold = 0.95,
                    BigramWeightMultiplier = 1.5,
                    ScoreMultiplier = 1.2
                },

                Mode.Strict => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 1,
                    MaxConsecutiveShortWords = 2,
                    SingleLetterPenalty = 1.5,
                    ShortWordPenalty = 1.2,
                    ShortWordGlobalPenalty = 0.3,
                    EstimatedScorePerWord = 3.5,
                    OptimismFactor = 0.8,
                    PotentialThreshold = 0.9,
                    BigramWeightMultiplier = 1.5,
                    ScoreMultiplier = 1.3
                },

                Mode.Soft => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 1,
                    MaxConsecutiveShortWords = 2,
                    SingleLetterPenalty = 0.8,
                    ShortWordPenalty = 0.5,
                    ShortWordGlobalPenalty = 0.2,
                    EstimatedScorePerWord = 3.2,
                    OptimismFactor = 1.0,
                    PotentialThreshold = 0.7,
                    BigramWeightMultiplier = 1.5,
                    ScoreMultiplier = 1.1
                },

                Mode.Lenient => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 1,
                    MaxConsecutiveShortWords = 5,
                    SingleLetterPenalty = 0.4,
                    ShortWordPenalty = 0.3,
                    ShortWordGlobalPenalty = 0.1,
                    EstimatedScorePerWord = 2.8,
                    OptimismFactor = 1.4,
                    PotentialThreshold = 0.5,
                    BigramWeightMultiplier = 2.0,
                    ScoreMultiplier = 1.0
                },

                Mode.Lenient_2 => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 1,
                    MaxConsecutiveShortWords = 6,
                    SingleLetterPenalty = 0.2,
                    ShortWordPenalty = 0.1,
                    ShortWordGlobalPenalty = 0.05,
                    EstimatedScorePerWord = 2.5,
                    OptimismFactor = 1.6,
                    PotentialThreshold = 0.3,
                    BigramWeightMultiplier = 2.5,
                    ScoreMultiplier = 0.9
                },

                Mode.UltraLenient => new RestorationConfig
                {
                    MaxConsecutiveSingleLetters = 2,
                    MaxConsecutiveShortWords = 8,
                    SingleLetterPenalty = 0.05,
                    ShortWordPenalty = 0.0,
                    ShortWordGlobalPenalty = 0.0,
                    EstimatedScorePerWord = 2.0,
                    OptimismFactor = 2.0,
                    PotentialThreshold = 0.2,
                    BigramWeightMultiplier = 3.0,
                    ScoreMultiplier = 0.7
                },

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
