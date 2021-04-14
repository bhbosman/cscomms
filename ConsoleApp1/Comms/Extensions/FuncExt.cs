using System;

namespace Comms.Extensions
{
    public static class FuncExt
    {
        public static Func<TSource, TResult> SelectWrapWithException<TSource, TResult>(this Func<TSource, TResult> action, Action<Exception> exception)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return 
                message =>
                {
                    try
                    {
                        return action(message);
                    }
                    catch (Exception e)
                    {
                        exception(e);
                        throw;
                    }
                };
        }
        
        public static Action<TSource> WrapWithException<TSource>(this Action<TSource> action, Action<Exception> exception)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return 
                message =>
                {
                    try
                    {
                        action(message);
                    }
                    catch (Exception e)
                    {
                        exception(e);
                    }
                };
        }
    }
}