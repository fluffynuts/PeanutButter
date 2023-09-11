using System;
using System.Threading.Tasks;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// What to do with an error
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    enum ErrorHandlerResult
{
    /// <summary>
    /// Return this when you have no error to work with
    /// </summary>
    NoError,

    /// <summary>
    /// Rethrow it
    /// </summary>
    Rethrow,

    /// <summary>
    /// Suppress it
    /// </summary>
    Suppress
}

/// <summary>
/// Provides a mechanism to surround an action with
/// a pre-action and post-action
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class Steps
{
    /// <summary>
    /// Runs the provided steps in order
    /// - before
    /// - activity
    /// - after
    ///
    /// * after is supplied any exception thrown by the activity - if the
    ///     activity completes without error, after will be called with null
    /// * errors in the before and after actions are not handled for you 
    /// </summary>
    /// <param name="before"></param>
    /// <param name="activity"></param>
    /// <param name="after"></param>
    public void Run(
        Action before,
        Action activity,
        Action<Exception> after
    )
    {
        Run(
            before,
            activity,
            ex =>
            {
                after(ex);
                return ErrorHandlerResult.Suppress;
            }
        );
    }

    /// <summary>
    /// Runs the three provided actions in order,
    /// providing any exception thrown by the first
    /// two actions to the final one which can decide on
    /// whether the error is fatal or not
    ///
    /// * after is supplied any exception thrown by the activity - if the
    ///     activity completes without error, after will be called with null.
    /// * after must direct the Steps what to do next:
    ///     NoError: do nothing (there was no error)
    ///     Rethrow: rethrow the error from the original scope
    ///     Suppress: suppress the error
    /// * errors in the before and after actions are not handled for you
    /// </summary>
    /// <param name="before"></param>
    /// <param name="activity"></param>
    /// <param name="after"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Run(
        Action before,
        Action activity,
        Func<Exception, ErrorHandlerResult> after
    )
    {
        before();
        try
        {
            activity();
            after(null);
        }
        catch (Exception ex)
        {
            if (after(ex) == ErrorHandlerResult.Rethrow)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Runs the provided steps in order
    /// - before
    /// - activity
    /// - after
    ///
    /// * after is supplied any exception thrown by the activity - if the
    ///     activity completes without error, after will be called with null
    /// * errors in the before and after actions are not handled for you 
    /// </summary>
    /// <param name="before"></param>
    /// <param name="activity"></param>
    /// <param name="after"></param>
    public async Task RunAsync(
        Func<Task> before,
        Func<Task> activity,
        Func<Exception, Task> after
    )
    {
        await RunAsync(
            before,
            activity,
            async e =>
            {
                await after(e);
                return ErrorHandlerResult.Rethrow;
            }
        );
    }

    /// <summary>
    /// Runs the three provided actions in order,
    /// providing any exception thrown by the first
    /// two actions to the final one which can return
    /// true to suppress the error or false to 
    /// </summary>
    /// <param name="before"></param>
    /// <param name="activity"></param>
    /// <param name="after"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task RunAsync(
        Func<Task> before,
        Func<Task> activity,
        Func<Exception, Task<ErrorHandlerResult>> after
    )
    {
        await before();
        try
        {
            await activity();
            await after(null);
        }
        catch (Exception ex)
        {
            var result = await after(ex);
            if (result == ErrorHandlerResult.Rethrow)
            {
                throw;
            }
        }
    }
}