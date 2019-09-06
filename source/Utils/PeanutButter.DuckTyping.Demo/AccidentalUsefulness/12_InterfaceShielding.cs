using System.ComponentModel;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.AccidentalUsefulness
{

    public class Coder
    {
        public int Id { get; set; } = 4;
        public string Name { get; set; } = "Jamie Fenton";
        public string Address { get; set; } = "123 Some Road, Some Town, Some Country";
        public string PhoneNumber { get; set; } = "555-1234";
    }

    [Order(12)]
    [Description(
        @"Sometimes we'd like to pass nearly all of an object
through to some other call -- but not quite all of it.
We may have different reasons for this, such as keeping
inferred dependencies simpler, or simply not making
some piece of data immediately accessible to the called
code")]
    public class InterfaceShielding : Demo
    {
        public override void Run()
        {
            var data = new Coder();
            var ducked = data.DuckAs<IReadOnlyEntity>();
            
            Log("Full data, containing sensitive fields\n", data);
            
            EmptyLine();
            
            Log("Shielded data:\n", ducked);
        }
    }
}