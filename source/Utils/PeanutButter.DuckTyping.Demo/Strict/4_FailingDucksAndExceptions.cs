using System.ComponentModel;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo.Strict
{
    [Order(4)]
    [Description(@"
Sometimes a failing duck returning null is fine.
Other times, we'd like to know _why_ the duck fails.")]
    public class FailingDucksAndExceptions : Demo
    {
        public override void Run()
        {
            var missingStuff = new
            {
                Id = 1
            };
            var attemptingWriteOnAnonymous = new
            {
                Id = 1,
                Name = "Grace Hopper"
            };
            
            try
            {
                missingStuff.DuckAs<IReadOnlyEntity>(true);
            }
            catch (UnDuckableException ex)
            {
                Log("Can't strict duck when missing properties:\n",
                    ex.Errors.JoinWith("\n"));
            }

            try
            {
                attemptingWriteOnAnonymous.DuckAs<IReadWriteEntity>(true);
            }
            catch (UnDuckableException ex)
            {
                EmptyLine();
                Log("Actually, we can't duck to anything which is more restrictive:");
                Log(ex.Errors.JoinWith("\n"));
            }
        }
    }
}