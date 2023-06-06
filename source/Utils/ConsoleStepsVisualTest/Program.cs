// See https://aka.ms/new-console-template for more information

using PeanutButter.Utils;

var steps = ConsoleSteps.Basic();
steps.Run(
    "milk the cow",
    () => Thread.Sleep(1000)
);

try
{
    steps.Run(
        "milk the rock",
        () =>
        {
            Thread.Sleep(1000);
            throw new NotImplementedException(
                "can't milk a rock, dude"
            );
        }
    );
    
}
catch
{
    // suppress
}