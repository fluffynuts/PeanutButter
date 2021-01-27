namespace PeanutButter.INI
{
    internal class StrictLineParser : BestEffortLineParser
    {
        protected override string Unescape(
            string data,
            bool containsEscapeEntities
        )
        {
            return data.IndexOf('\\') > -1
                ? ApplyEscapeSequences(data)
                : data;
        }
    }
}