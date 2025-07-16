// <copyright file="ResultSideEffectExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Zentient.Results
{
    /// <summary>
    /// Provides a versatile set of extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate fluent, expressive, and consistent functional-style side-effecting operations
    /// based on the result's success or failure state, or unconditionally.
    /// </summary>
    public static class ResultSideEffectExtensions
    {
        #region OnSuccess Extensions (Conditional on Success)

        /// <summary>
        /// Executes an action if the non-generic <see cref="IResult"/> is successful.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The action to execute if the result is successful.</param>
        /// <returns>The original <see cref="IResult"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult OnSuccess(this IResult result, Action onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                onSuccess();
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action if the non-generic <see cref="IResult"/> is successful.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The asynchronous action to execute if the result is successful.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static async Task<IResult> OnSuccessAsync(this IResult result, Func<Task> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                await onSuccess().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Executes an action if the generic <see cref="IResult{TValue}"/> is successful,
        /// providing the encapsulated value to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The action to execute, receiving the success value.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnSuccess<TValue>(this IResult<TValue> result, Action<TValue> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                onSuccess(result.Value!);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action if the generic <see cref="IResult{TValue}"/> is successful,
        /// providing the encapsulated value to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The asynchronous action to execute, receiving the success value.</param>
        /// <returns>A <see cref="Task{IResult{TValue}}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static async Task<IResult<TValue>> OnSuccessAsync<TValue>(this IResult<TValue> result, Func<TValue, Task> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                await onSuccess(result.Value!).ConfigureAwait(false);
            }

            return result;
        }

        #endregion

        #region OnFailure Extensions (Conditional on Failure)

        /// <summary>
        /// Executes an action if the non-generic <see cref="IResult"/> is a failure,
        /// providing the list of <see cref="ErrorInfo"/> to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">The action to execute, receiving the list of errors.</param>
        /// <returns>The original <see cref="IResult"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult OnFailure(this IResult result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action if the non-generic <see cref="IResult"/> is a failure,
        /// providing the list of <see cref="ErrorInfo"/> to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">The asynchronous action to execute, receiving the list of errors.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static async Task<IResult> OnFailureAsync(this IResult result, Func<IReadOnlyList<ErrorInfo>, Task> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (result.IsFailure)
            {
                await onFailure(result.Errors).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Executes an action if the generic <see cref="IResult{TValue}"/> is a failure,
        /// providing the list of <see cref="ErrorInfo"/> to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">The action to execute, receiving the list of errors.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnFailure<TValue>(this IResult<TValue> result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action if the generic <see cref="IResult{TValue}"/> is a failure,
        /// providing the list of <see cref="ErrorInfo"/> to the action.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">The asynchronous action to execute, receiving the list of errors.</param>
        /// <returns>A <see cref="Task{IResult{TValue}}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static async Task<IResult<TValue>> OnFailureAsync<TValue>(this IResult<TValue> result, Func<IReadOnlyList<ErrorInfo>, Task> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (result.IsFailure)
            {
                await onFailure(result.Errors).ConfigureAwait(false);
            }

            return result;
        }

        #endregion

        #region Finally Extensions (Unconditional Execution)

        /// <summary>
        /// Executes an action regardless of whether the non-generic <see cref="IResult"/> is a success or a failure.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onCompletion">The action to execute.</param>
        /// <returns>The original <see cref="IResult"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onCompletion"/> is <c>null</c>.</exception>
        public static IResult Finally(this IResult result, Action onCompletion)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onCompletion);

            onCompletion();
            return result;
        }

        /// <summary>
        /// Asynchronously executes an action regardless of whether the non-generic <see cref="IResult"/> is a success or a failure.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onCompletion">The asynchronous action to execute.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onCompletion"/> is <c>null</c>.</exception>
        public static async Task<IResult> FinallyAsync(this IResult result, Func<Task> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onCompletion);

            await onCompletion().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Executes an action regardless of whether the generic <see cref="IResult{TValue}"/> is a success or a failure.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onCompletion">The action to execute, receiving the result instance itself.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onCompletion"/> is <c>null</c>.</exception>
        public static IResult<TValue> Finally<TValue>(this IResult<TValue> result, Action<IResult<TValue>> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onCompletion);

            onCompletion(result);
            return result;
        }

        /// <summary>
        /// Asynchronously executes an action regardless of whether the generic <see cref="IResult{TValue}"/> is a success or a failure.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onCompletion">The asynchronous action to execute, receiving the result instance itself.</param>
        /// <returns>A <see cref="Task{IResult{TValue}}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onCompletion"/> is <c>null</c>.</exception>
        public static async Task<IResult<TValue>> FinallyAsync<TValue>(this IResult<TValue> result, Func<IResult<TValue>, Task> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onCompletion);

            await onCompletion(result).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Tap Extensions (Alias for OnSuccess for IResult<T>)

        /// <summary>
        /// Executes an action if the generic <see cref="IResult{TValue}"/> is successful,
        /// providing the encapsulated value to the action. This is an alias for <see cref="OnSuccess{TValue}(IResult{TValue}, Action{TValue})"/>.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="tapAction">The action to execute, receiving the success value.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for continued chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="tapAction"/> is <c>null</c>.</exception>
        public static IResult<TValue> Tap<TValue>(this IResult<TValue> result, Action<TValue> tapAction) =>
            result.OnSuccess(tapAction);

        /// <summary>
        /// Asynchronously executes an action if the generic <see cref="IResult{TValue}"/> is successful,
        /// providing the encapsulated value to the action. This is an alias for <see cref="OnSuccessAsync{TValue}(IResult{TValue}, Func{TValue, Task})"/>.
        /// This method allows for fluent side-effect chaining without altering the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="tapAction">The asynchronous action to execute, receiving the success value.</param>
        /// <returns>A <see cref="Task{IResult{TValue}}"/> representing the asynchronous operation, returning the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="tapAction"/> is <c>null</c>.</exception>
        public static Task<IResult<TValue>> TapAsync<TValue>(this IResult<TValue> result, Func<TValue, Task> tapAction) =>
            result.OnSuccessAsync(tapAction);

        #endregion
    }
}
