// <copyright file="ResultTryExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

// Assuming Zentient.Results.Constants and Zentient.Results.Serialization are correctly referenced
// and contain ErrorCodes and MetadataKeys if used.

namespace Zentient.Results
{
    /// <summary>
    /// Provides a comprehensive set of extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate fluent, expressive, and consistent functional-style error handling and result manipulation
    /// by wrapping operations in try-catch blocks.
    /// </summary>
    public static class ResultTryExtensions
    {
        #region Synchronous Try Methods

        /// <summary>
        /// Wraps an action into a non-generic <see cref="IResult"/>, catching any exception and returning a failure result if an exception occurs.
        /// The caught exception will be converted into an <see cref="ErrorInfo"/> using <see cref="ErrorInfo.FromException(Exception, string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing error information from the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public static IResult Try(this Action action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            try
            {
                action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorInfo.FromException(ex));
            }
        }

        /// <summary>
        /// Wraps an action into a non-generic <see cref="IResult"/>, catching a specific type of exception and returning a failure result.
        /// Other exception types will be re-thrown.
        /// </summary>
        /// <typeparam name="TException">The specific type of exception to catch.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="errorHandler">A function to transform the caught exception into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> or <paramref name="errorHandler"/> is null.</exception>
        public static IResult Try<TException>(this Action action, Func<TException, ErrorInfo> errorHandler)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                action();
                return Result.Success();
            }
            catch (TException ex)
            {
                return Result.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps an action into a non-generic <see cref="IResult"/>, catching any exception and allowing for custom error mapping.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="errorHandler">A function to transform the caught <see cref="Exception"/> into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> or <paramref name="errorHandler"/> is null.</exception>
        public static IResult Try(this Action action, Func<Exception, ErrorInfo> errorHandler)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps a function into a generic <see cref="IResult{T}"/>, catching any exception and returning a failure result if an exception occurs.
        /// The caught exception will be converted into an <see cref="ErrorInfo"/> using <see cref="ErrorInfo.FromException(Exception, string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>A successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing error information from the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public static IResult<T> Try<T>(this Func<T> func)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));

            try
            {
                var value = func();
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ErrorInfo.FromException(ex));
            }
        }

        /// <summary>
        /// Wraps a function into a generic <see cref="IResult{T}"/>, catching a specific type of exception and returning a failure result.
        /// Other exception types will be re-thrown.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <typeparam name="TException">The specific type of exception to catch.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="errorHandler">A function to transform the caught exception into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static IResult<T> Try<T, TException>(this Func<T> func, Func<TException, ErrorInfo> errorHandler)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                var value = func();
                return Result<T>.Success(value);
            }
            catch (TException ex)
            {
                return Result<T>.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps a function into a generic <see cref="IResult{T}"/>, catching any exception and allowing for custom error mapping.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="errorHandler">A function to transform the caught <see cref="Exception"/> into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static IResult<T> Try<T>(this Func<T> func, Func<Exception, ErrorInfo> errorHandler)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                var value = func();
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(errorHandler(ex));
            }
        }

        #endregion

        #region Asynchronous Try Methods

        /// <summary>
        /// Wraps an asynchronous action into a non-generic <see cref="IResult"/>, catching any exception and returning a failure result if an exception occurs.
        /// The caught exception will be converted into an <see cref="ErrorInfo"/> using <see cref="ErrorInfo.FromException(Exception, string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>.
        /// </summary>
        /// <param name="func">The asynchronous action to execute (a <see cref="Func{Task}"/>).</param>
        /// <returns>A <see cref="Task{IResult}"/> representing a successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing error information from the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public static async Task<IResult> Try(this Func<Task> func)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));

            try
            {
                await func().ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorInfo.FromException(ex));
            }
        }

        /// <summary>
        /// Wraps an asynchronous action into a non-generic <see cref="IResult"/>, catching a specific type of exception.
        /// Other exception types will be re-thrown.
        /// </summary>
        /// <typeparam name="TException">The specific type of exception to catch.</typeparam>
        /// <param name="func">The asynchronous action to execute (a <see cref="Func{Task}"/>).</param>
        /// <param name="errorHandler">A function to transform the caught exception into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing a successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static async Task<IResult> Try<TException>(this Func<Task> func, Func<TException, ErrorInfo> errorHandler)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                await func().ConfigureAwait(false);
                return Result.Success();
            }
            catch (TException ex)
            {
                return Result.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps an asynchronous action into a non-generic <see cref="IResult"/>, catching any exception and allowing for custom error mapping.
        /// </summary>
        /// <param name="func">The asynchronous action to execute (a <see cref="Func{Task}"/>).</param>
        /// <param name="errorHandler">A function to transform the caught <see cref="Exception"/> into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing a successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static async Task<IResult> Try(this Func<Task> func, Func<Exception, ErrorInfo> errorHandler)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                await func().ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps an asynchronous function into a generic <see cref="IResult{T}"/>, catching any exception and returning a failure result.
        /// The caught exception will be converted into an <see cref="ErrorInfo"/> using <see cref="ErrorInfo.FromException(Exception, string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The asynchronous function to execute (a <see cref="Func{Task{T}}"/>).</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing a successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing error information from the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public static async Task<IResult<T>> Try<T>(this Func<Task<T>> func)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));

            try
            {
                var value = await func().ConfigureAwait(false);
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ErrorInfo.FromException(ex));
            }
        }

        /// <summary>
        /// Wraps an asynchronous function into a generic <see cref="IResult{T}"/>, catching a specific type of exception.
        /// Other exception types will be re-thrown.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <typeparam name="TException">The specific type of exception to catch.</typeparam>
        /// <param name="func">The asynchronous function to execute (a <see cref="Func{Task{T}}"/>).</param>
        /// <param name="errorHandler">A function to transform the caught exception into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing a successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static async Task<IResult<T>> Try<T, TException>(this Func<Task<T>> func, Func<TException, ErrorInfo> errorHandler)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                var value = await func().ConfigureAwait(false);
                return Result<T>.Success(value);
            }
            catch (TException ex)
            {
                return Result<T>.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        /// Wraps an asynchronous function into a generic <see cref="IResult{T}"/>, catching any exception and allowing for custom error mapping.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The asynchronous function to execute (a <see cref="Func{Task{T}}"/>).</param>
        /// <param name="errorHandler">A function to transform the caught <see cref="Exception"/> into an <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing a successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing custom error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="errorHandler"/> is null.</exception>
        public static async Task<IResult<T>> Try<T>(this Func<Task<T>> func, Func<Exception, ErrorInfo> errorHandler)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(errorHandler, nameof(errorHandler));

            try
            {
                var value = await func().ConfigureAwait(false);
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(errorHandler(ex));
            }
        }

        #endregion

        #region Try with Finally (Cleanup)

        /// <summary>
        /// Wraps an action into a non-generic <see cref="IResult"/>, executing a finally action regardless of success or failure.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="finallyAction">The action to execute in a finally block.</param>
        /// <returns>A successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> or <paramref name="finallyAction"/> is null.</exception>
        public static IResult TryFinally(this Action action, Action finallyAction)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            ArgumentNullException.ThrowIfNull(finallyAction, nameof(finallyAction));

            try
            {
                action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorInfo.FromException(ex));
            }
            finally
            {
                finallyAction();
            }
        }

        /// <summary>
        /// Wraps a function into a generic <see cref="IResult{T}"/>, executing a finally action regardless of success or failure.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="finallyAction">The action to execute in a finally block.</param>
        /// <returns>A successful <see cref="IResult{T}"/> if the function completes without exception; otherwise, a failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="finallyAction"/> is null.</exception>
        public static IResult<T> TryFinally<T>(this Func<T> func, Action finallyAction)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(finallyAction, nameof(finallyAction));

            try
            {
                var value = func();
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ErrorInfo.FromException(ex));
            }
            finally
            {
                finallyAction();
            }
        }

        /// <summary>
        /// Wraps an asynchronous action into a non-generic <see cref="IResult"/>, executing an asynchronous finally action regardless of success or failure.
        /// </summary>
        /// <param name="func">The asynchronous action to execute.</param>
        /// <param name="finallyFunc">The asynchronous action to execute in a finally block.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing a successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="finallyFunc"/> is null.</exception>
        public static async Task<IResult> TryFinally(this Func<Task> func, Func<Task> finallyFunc)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(finallyFunc, nameof(finallyFunc));

            try
            {
                await func().ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorInfo.FromException(ex));
            }
            finally
            {
                await finallyFunc().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Wraps an asynchronous function into a generic <see cref="IResult{T}"/>, executing an asynchronous finally action regardless of success or failure.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The asynchronous function to execute.</param>
        /// <param name="finallyFunc">The asynchronous action to execute in a finally block.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing a successful <see cref="IResult{T}"/> if the function completes without exception; otherwise, a failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> or <paramref name="finallyFunc"/> is null.</exception>
        public static async Task<IResult<T>> TryFinally<T>(this Func<Task<T>> func, Func<Task> finallyFunc)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            ArgumentNullException.ThrowIfNull(finallyFunc, nameof(finallyFunc));

            try
            {
                var value = await func().ConfigureAwait(false);
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ErrorInfo.FromException(ex));
            }
            finally
            {
                await finallyFunc().ConfigureAwait(false);
            }
        }

        #endregion
    }
}
