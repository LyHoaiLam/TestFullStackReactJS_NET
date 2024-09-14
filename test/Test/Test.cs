namespace YourNamespace.Services
{
    public class Test
    {
        public int AverageLettersPerWord(string sentence)
    {
        string[] words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var filteredWords = words.Where(word => word.Length >= 2).ToList();

        if (filteredWords.Count == 0)
        {
            return 0;
        }

        int totalLetters = filteredWords.Sum(word => word.Length);

        return totalLetters / filteredWords.Count;
    }
    }
}
