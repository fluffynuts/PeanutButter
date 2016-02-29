namespace PeanutButter.RandomGenerators
{
    public interface IBuilder<TSubject>
    {
        TSubject Build();
    }
    public abstract class BuilderBase<TConcrete, TSubject> where TConcrete: IBuilder<TSubject>, new()
    {
        public static TConcrete Create()
        {
            return new TConcrete();
        }
        public static TSubject BuildDefault()
        {
            return Create().Build();
        }
    }
}
