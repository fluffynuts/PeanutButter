using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NSubstitute.Core;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic;

/// <summary>
/// Provides extensions for substitutes to facilitate easier
/// interrogation of the call history, eg when you want to
/// pull an argument for an invocation to test against
/// </summary>
public static class SubstituteExtensions
{
    /// <summary>
    /// Retrieve all received calls for the provided
    /// substitute, filtered by the provided filter
    /// </summary>
    /// <param name="substitute"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static CallWrapper[] ReceivedCalls(
        this object substitute,
        Func<CallWrapper, bool> filter
    )
    {
        return substitute.ReceivedCalls()
            .Select(CallWrapper.From)
            .Where(filter)
            .ToArray();
    }

    /// <summary>
    /// Retrieve all recorded calls for the provided method
    /// on the provided substitute
    /// </summary>
    /// <param name="substitute"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static CallWrapper[] ReceivedCalls(
        this object substitute,
        string methodName
    )
    {
        if (substitute is null)
        {
            throw new ArgumentNullException(nameof(substitute));
        }

        if (substitute.DoesNotHaveUniqueMethodCalled(methodName))
        {
            throw new ArgumentException(
                $"{substitute.GetType()} has more than one method named '{methodName}' - you'll have to use one of the other helpers.",
                nameof(methodName)
            );
        }

        return substitute.ReceivedCalls()
            .Where(
                o => o.GetMethodInfo().Name == methodName
            ).Select(CallWrapper.From)
            .ToArray();
    }

    /// <summary>
    /// Tests if a type or object only has one method with the provided name
    /// (ie no overloads, and a method with that name exists)
    /// </summary>
    /// <param name="objOrType"></param>
    /// <param name="seek"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool DoesNotHaveUniqueMethodCalled(
        this object objOrType,
        string seek
    )
    {
        if (objOrType is null)
        {
            throw new ArgumentNullException(nameof(objOrType));
        }
        var type = objOrType.IsRuntimeType()
            ? (Type)objOrType
            : objOrType.GetType();
        
        var methods = type.GetMethods()
            .Where(o => o.Name == seek)
            .ToArray();
        return methods.Length != 1;
    }

    /// <summary>
    /// Wraps an NSubstitute ICall
    /// </summary>
    public class CallWrapper
    {
        internal static CallWrapper From(ICall call)
        {
            return new(call);
        }

        /// <summary>
        /// The method which was invoked
        /// </summary>
        public MethodInfo MethodInfo =>
            _methodInfo ??= CallInfo.GetMethodInfo();

        private MethodInfo _methodInfo;

        /// <summary>
        /// The name of the method that was invoked
        /// </summary>
        public string MethodName => MethodInfo?.Name;

        /// <summary>
        /// The arguments supplied to this call
        /// </summary>
        public object[] Arguments =>
            _arguments ??= CallInfo.GetArguments();

        private object[] _arguments;

        /// <summary>
        /// The raw ICall info from NSubstitute
        /// </summary>
        public ICall CallInfo { get; }

        /// <summary>
        /// Constructs a new instance of the CallWrapper
        /// </summary>
        /// <param name="call"></param>
        public CallWrapper(ICall call)
        {
            CallInfo = call;
        }
    }
}