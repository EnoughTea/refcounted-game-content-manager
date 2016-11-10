using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;

namespace MonoCustomContentManagers {
    /// <summary> Extension methods for delegates. </summary>
    internal static class DelegateExtensions
    {
        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        /// <param name="args">The arguments associated with the event.</param>
        public static void Call(
            this EventHandler handler,
            object sender,
            EventArgs args)
        {
            //  Avoid a mutation between the reads.
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(sender, args);
        }

        /// <summary>Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null.</summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        public static void Call(this EventHandler handler, object sender = null)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <typeparam name="TArgs">The type of the arguments associated with the event.</typeparam>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        /// <param name="args">The arguments associated with the event.</param>
        public static void Call<TArgs>(
            this EventHandler<TArgs> handler,
            object sender,
            TArgs args)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(sender, args);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <typeparam name="TArgs">The type of the arguments associated with the event.</typeparam>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="args">The arguments associated with the event.</param>
        public static void Call<TArgs>(this EventHandler<TArgs> handler, TArgs args)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(null, args);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        public static void Call(this Action handler)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke();
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg">The argument associated with the event.</param>
        public static void Call<TArg>(this Action<TArg> handler, TArg arg)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        public static void Call<TArg1, TArg2>(
            this Action<TArg1, TArg2> handler,
            TArg1 arg1,
            TArg2 arg2)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg1, arg2);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        public static void Call<TArg1, TArg2, TArg3>(
            this Action<TArg1, TArg2, TArg3> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg1, arg2, arg3);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        public static void Call<TArg1, TArg2, TArg3, TArg4>(
            this Action<TArg1, TArg2, TArg3, TArg4> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg1, arg2, arg3, arg4);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        public static void Call<TArg1, TArg2, TArg3, TArg4, TArg5>(
            this Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4,
            TArg5 arg5)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        /// <param name="arg6">The sixth argument associated with the event.</param>
        public static void Call<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4,
            TArg5 arg5,
            TArg6 arg6)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null.</summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TResult>(
            this Func<TResult> handler,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler() : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg">The argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg, TResult>(
            this Func<TArg, TResult> handler,
            TArg arg,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg) : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg1, TArg2, TResult>(
            this Func<TArg1, TArg2, TResult> handler,
            TArg1 arg1,
            TArg2 arg2,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg1, arg2) : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg1, TArg2, TArg3, TResult>(
            this Func<TArg1, TArg2, TArg3, TResult> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg1, arg2, arg3) : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg1, TArg2, TArg3, TArg4, TResult>(
            this Func<TArg1, TArg2, TArg3, TArg4, TResult> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg1, arg2, arg3, arg4) : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
            this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4,
            TArg5 arg5,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg1, arg2, arg3, arg4, arg5) : defaultResult;
        }

        /// <summary> Performs invocation of the delegate making sure that it exists first.
        /// Can be called on null. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        /// <param name="arg6">The sixth argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Call<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
            this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> handler,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            TArg4 arg4,
            TArg5 arg5,
            TArg6 arg6,
            TResult defaultResult = default(TResult))
        {
            var currentHandler = Volatile.Read(ref handler);
            return currentHandler != null ? currentHandler(arg1, arg2, arg3, arg4, arg5, arg6) : defaultResult;
        }

        /// <summary> Performs invocation of the <see cref="PropertyChangedEventHandler" /> making sure
        /// that it exists first. Can be called on null. </summary>
        /// <param name="handler">The event handler, can be null.</param>
        /// <param name="instance">The object which raises the event.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        public static void Call(
            this PropertyChangedEventHandler handler,
            object instance,
            string propertyName)
        {
            var currentHandler = Volatile.Read(ref handler);
            currentHandler?.Invoke(instance, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary> Performs invocation of the <see cref="PropertyChangedEventHandler" /> making sure
        /// that it exists first. Can be called on null. </summary>
        /// <typeparam name="T">The type of the property that was changed.</typeparam>
        /// <param name="handler">The event handler, can be null.</param>
        /// <param name="propertyGetterExpression">An expression that returns the value of the property.</param>
        /// <exception cref="ArgumentException">Property getter expression appears not to get any property</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyGetterExpression"/> is <see langword="null"/>
        /// </exception>
        public static void Call<T>(
            this PropertyChangedEventHandler handler,
            Expression<Func<T>> propertyGetterExpression)
        {
            if (propertyGetterExpression == null) {
                throw new ArgumentNullException(nameof(propertyGetterExpression));
            }

            var currentHandler = Volatile.Read(ref handler);
            if (currentHandler != null) {
                MemberExpression expression;
                try {
                    expression = (MemberExpression)propertyGetterExpression.Body;
                } catch (InvalidCastException) {
                    throw new ArgumentException("Property getter expression appears not to get any property",
                                                nameof(propertyGetterExpression));
                }

                var instanceExpression = (ConstantExpression)expression.Expression;
                var propertyName = expression.Member.Name;
                currentHandler(instanceExpression.Value, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
