﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
// ReSharper disable UnusedType.Global

namespace PeanutButter.Utils;

/// <summary>
/// Provides a mechanism to make debugging an application
/// a little easier:
/// using the disposable pattern, surround a block
/// of code with the scope for this item's disposal
/// and you'll get:
/// 1. Execution will wait for a debugger to be attached
/// 2. The block will be locked for the duration of the
///    scope so that you don't have to deal with contending
///    threads
/// Usage: create a using block around the code you want to
///        debug and ensure you have a breakpoint in that block
///        then start up your application and attach a debugger
/// WARNING: since the code will wait for an attached
/// debugger, you should _not_ leave this unattended
/// </summary>
public class SingleDebuggerBlock : IDisposable
{
    private readonly SemaphoreSlim _lock;
    private static readonly Dictionary<string, SemaphoreSlim> Locks = new();

    /// <summary>
    /// Constructs the block with the provided identifier
    /// </summary>
    /// <param name="identifier"></param>
    public SingleDebuggerBlock(string identifier)
    {
        lock (Locks)
        {
            if (Locks.TryGetValue(identifier, out var lck))
            {
                _lock = lck;
                _lock.Wait();
            }
            else
            {
                _lock = new SemaphoreSlim(1);
                Locks[identifier] = _lock;
                _lock.Wait();
            }
        }

        WaitForDebuggerToAttach(identifier);
    }

    private void WaitForDebuggerToAttach(
        string identifier
    )
    {
        var haveWarned = false;
        while (!Debugger.IsAttached)
        {
            if (!haveWarned)
            {
                Console.Error.WriteLine(
                    $"""
                    Waiting for debugger to attach for block: '{identifier}'
                    """
                );
                haveWarned = true;
            }

            Thread.Sleep(100);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Release();
    }
}