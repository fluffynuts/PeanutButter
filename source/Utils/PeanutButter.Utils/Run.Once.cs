using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable once UnusedType.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides convenience wrapper functions to run small bits of work
    /// in parallel threads, not tasks. No async/await, no Tasks (unless
    /// you want to return them and await them yourself). No TPL. No
    /// handbrakes. No magic context. No guarantees, except that your
    /// work will be executed. May make the host machine very tired.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static partial class Run
    {
        /// <summary>
        /// Runs the action once and once only
        /// </summary>
        /// <param name="gateFlag"></param>
        /// <param name="toRun"></param>
        public static void Once(
            ref bool gateFlag,
            Action toRun
        )
        {
            if (gateFlag)
            {
                return;
            }
            
            gateFlag = true;

            toRun();
        }
    }
}
