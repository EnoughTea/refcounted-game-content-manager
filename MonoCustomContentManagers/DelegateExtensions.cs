using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading;

namespace MonoCustomContentManagers {
    /// <summary> Extension methods for delegates. </summary>
    internal static class DelegateExtensions {
        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        /// <param name="args">The arguments associated with the event.</param>
        public static void Raise(this EventHandler handler, object sender, EventArgs args) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(sender, args);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        public static void Raise(this EventHandler handler, object sender) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary> Raises an event. </summary>
        /// <typeparam name="TArgs">The type of the arguments associated with the event.</typeparam>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="sender">The object which raises the event.</param>
        /// <param name="args">The arguments associated with the event.</param>
        public static void Raise<TArgs>(this EventHandler<TArgs> handler, object sender, TArgs args)
            where TArgs : EventArgs {
            Contract.Requires(sender != null);
            Contract.Requires(args != null);

            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(sender, args);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        public static void Raise(this Action handler) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke();
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg">The argument associated with the event.</param>
        public static void Raise<TArg>(this Action<TArg> handler, TArg arg) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(arg);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        public static void Raise<TArg1, TArg2>(this Action<TArg1, TArg2> handler, TArg1 arg1, TArg2 arg2) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(arg1, arg2);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        public static void Raise<TArg1, TArg2, TArg3>(this Action<TArg1, TArg2, TArg3> handler, TArg1 arg1, TArg2 arg2,
            TArg3 arg3) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(arg1, arg2, arg3);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        public static void Raise<TArg1, TArg2, TArg3, TArg4>(
            this Action<TArg1, TArg2, TArg3, TArg4> handler,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(arg1, arg2, arg3, arg4);
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        public static void Raise<TArg1, TArg2, TArg3, TArg4, TArg5>(
            this Action<TArg1, TArg2, TArg3, TArg4, TArg5>
            handler, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>Raises an event.</summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TResult>(this Func<TResult> handler,
            TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler() : defaultResult;
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg">The argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TArg, TResult>(this Func<TArg, TResult> handler, TArg arg,
            TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler(arg) : defaultResult;
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, TResult> handler,
            TArg1 arg1, TArg2 arg2, TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler(arg1, arg2) : defaultResult;
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TArg1, TArg2, TArg3, TResult>(
            this Func<TArg1, TArg2, TArg3, TResult> handler,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler(arg1, arg2, arg3) : defaultResult;
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TArg1, TArg2, TArg3, TArg4, TResult>(
            this Func<TArg1, TArg2, TArg3, TArg4, TResult>
            handler, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4,
            TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler(arg1, arg2, arg3, arg4) : defaultResult;
        }

        /// <summary> Raises an event. </summary>
        /// <param name="handler">The event handler to be invoked.</param>
        /// <param name="arg1">The first argument associated with the event.</param>
        /// <param name="arg2">The second argument associated with the event.</param>
        /// <param name="arg3">The third argument associated with the event.</param>
        /// <param name="arg4">The fourth argument associated with the event.</param>
        /// <param name="arg5">The fifth argument associated with the event.</param>
        /// <param name="defaultResult">The default result to use when handler was null.</param>
        /// <returns>Event result.</returns>
        public static TResult Raise<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
            this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> handler, TArg1 arg1, TArg2 arg2,
            TArg3 arg3, TArg4 arg4, TArg5 arg5, TResult defaultResult = default(TResult)) {
            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            return (currentHandler != null) ? currentHandler(arg1, arg2, arg3, arg4, arg5) : defaultResult;
        }

        /// <summary> Raises an event based on <see cref="PropertyChangedEventHandler" />. </summary>
        /// <param name="handler">The event handler, can be null.</param>
        /// <param name="instance">The object which raises the event.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        public static void Raise(this PropertyChangedEventHandler handler, object instance,
            string propertyName) {
            Contract.Requires(instance != null);
            Contract.Requires(!string.IsNullOrEmpty(propertyName));

            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            currentHandler?.Invoke(instance, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary> Raises an event based on <see cref="PropertyChangedEventHandler" />. </summary>
        /// <typeparam name="T">The type of the property that was changed.</typeparam>
        /// <param name="handler">The event handler, can be null.</param>
        /// <param name="propertyGetterExpression">An expression that returns the value of the property.</param>
        public static void Raise<T>(this PropertyChangedEventHandler handler,
            Expression<Func<T>> propertyGetterExpression) {
            Contract.Requires(propertyGetterExpression != null);

            var currentHandler = Interlocked.CompareExchange(ref handler, null, null);
            if (currentHandler != null) {
                var expression = (MemberExpression)propertyGetterExpression.Body;
                var instanceExpression = (ConstantExpression)expression.Expression;
                string propertyName = expression.Member.Name;
                currentHandler(instanceExpression.Value, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
