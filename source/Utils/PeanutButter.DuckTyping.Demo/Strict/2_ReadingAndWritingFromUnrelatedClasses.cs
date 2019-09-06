using System.ComponentModel;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Strict
{
    public interface IReadWriteEntity
    {
        int Id { get; set; }
        string Name { get; set; }
    }
    
    [Order(2)]
    [Description("What about writing to the underlying object?")]
    public class ReadingAndWritingFromUnrelatedClasses : Demo
    {
        public override void Run()
        {
            var unrelatedObject = new LooksLikeAnEntity()
            {
                Id = 42,
                Name = "John Carmack"
            };
            var ducked = unrelatedObject.DuckAs<IReadWriteEntity>();
            Log("Before:\n", ducked);
            
            // Ada Lovelace came before all the great female programmers!
            ducked.Id = 0;
            ducked.Name = "Ada Lovelace";
            
            Log("After:\n", ducked);
            Log("After, original POCO:\n", unrelatedObject);
        }
    }
}