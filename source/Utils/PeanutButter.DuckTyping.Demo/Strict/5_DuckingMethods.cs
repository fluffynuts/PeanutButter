using System;
using System.ComponentModel;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Strict
{
    public interface IDuck
    {
        void Quack();
        void Walk(int distance);
    }

    public class FrenchDuck
    {
        public void Quack()
        {
            Console.WriteLine("\nLE QUACK!\n");
        }

        public void Walk(int howFar)
        {
            Console.WriteLine($"\nCarried my baguette {howFar}m\n");
        }
    }

    [Order(5)]
    [Description("We can also duck-type methods!")]
    public class DuckingMethods : Demo
    {
        public override void Run()
        {
            var Dominique = new FrenchDuck();
            var duck = Dominique.DuckAs<IDuck>();

            Log("Now we invoke the duck...");
            
            duck.Quack();
            EmptyLine();
            duck.Walk(3);
        }
    }
}