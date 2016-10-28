using System;

namespace PeanutButter.DuckTyping.AutoConversion
{
    public interface IConverter<T1, T2>: IConverter
    {
        T1 Convert(T2 input);
        T2 Convert(T1 input);
    }

    public interface IConverter
    {
        Type T1 { get; }
        Type T2 { get; }
    }
}