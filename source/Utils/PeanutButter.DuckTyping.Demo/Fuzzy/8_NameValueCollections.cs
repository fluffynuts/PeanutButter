using System.ComponentModel;
using System.Configuration;
using System.Linq;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo.Fuzzy
{
    public interface IAppConfig
    {
        string DatabaseServer { get; }
        string DatabaseDatabase { get; }
        string DatabaseUser { get; }
        string DatabasePassword { get; }
        bool MooDoTheStuff { get; }
        int MooEatTheThings { get; }
    }

    [Order(8)]
    [Description(
        @"One of the most useful places to use duck-typing is against configuration")]
    public class NameValueCollections : Demo
    {
        public override void Run()
        {
            var settings = ConfigurationManager.AppSettings.FuzzyDuckAs<IAppConfig>(true);
            Log("Raw Settings\n:",
                ConfigurationManager.AppSettings.AllKeys
                    .Select(k => $"{k} = {ConfigurationManager.AppSettings[k]}")
                    .JoinWith("\n")
            );
            EmptyLine();
            Log("Ducked\n:", settings);
            EmptyLine();
            Log("(Fuzzy ducking will skip over the following characters: . - _ :)");
        }
    }
}