// <copyright file="ResultTransformationExtensions.cs" company="Zentient Framework Team">
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
    /// Provides a comprehensive set of extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate fluent, expressive, and consistent functional-style transformations and chaining
    /// of results (Map, Bind, Match).
    /// </summary>
    public static class ResultTransformationExtensions
    {
        #region Map Extensions (Value Transformation)

        /// <summary>
        /// Transforms the success value of a generic <see cref="IResult{TIn}"/> to a new type <typeparamref name="TOut"/>.
        /// If the result is a failure, the errors and status propagate unchanged.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="selector">A function to map from <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.</param>
        /// <returns>A new <see cref="IResult{TOut}"/> with the transformed value if successful, or the original failure information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="selector"/> is <c>null</c>.</exception>
        public static IResult<TOut> Map<TIn, TOut>(this IResult<TIn> result, Func<TIn, TOut> selector)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(selector);

            return result.IsSuccess
                ? Result<TOut>.Success(value: selector(result.Value!), messages: result.Messages)
                : Result<TOut>.Failure(value: default, result.Errors);
        }

        /// <summary>
        /// Asynchronously transforms the success value of a generic <see cref="IResult{TIn}"/> to a new type <typeparamref name="TOut"/>.
        /// If the result is a failure, the errors and status propagate unchanged.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="selector">An asynchronous function to map from <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.</param>
        /// <returns>A <see cref="Task{IResult{TOut}}"/> representing the asynchronous operation, yielding a new <see cref="IResult{TOut}"/> with the transformed value if successful, or the original failure information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="selector"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> MapAsync<TIn, TOut>(this IResult<TIn> result, Func<TIn, Task<TOut>> selector)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(selector);

            return result.IsSuccess
                ? Result<TOut>.Success(value: await selector(result.Value!).ConfigureAwait(false), messages: result.Messages)
                : Result<TOut>.Failure(value: default, result.Errors);
        }

        /// <summary>
        /// Transforms a non-generic successful <see cref="IResult"/> into a generic <see cref="IResult{TOut}"/> with a specified value.
        /// If the original result is a failure, the errors and status propagate unchanged.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The source non-generic result.</param>
        /// <param name="value">The value to encapsulate in the new successful result.</param>
        /// <returns>A new <see cref="IResult{TOut}"/> with the specified value if successful, or the original failure information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IResult<TOut> Map<TOut>(this IResult result, TOut value)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.IsSuccess
                ? Result<TOut>.Success(value: value, messages: result.Messages)
                : Result<TOut>.Failure(value: default, result.Errors);
        }

        /// <summary>
        /// Asynchronously transforms a non-generic successful <see cref="IResult"/> into a generic <see cref="IResult{TOut}"/> by
        /// asynchronously providing a value. If the original result is a failure, the errors and status propagate unchanged.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The source non-generic result.</param>
        /// <param name="valueProvider">An asynchronous function that provides the value to encapsulate in the new successful result.</param>
        /// <returns>A <see cref="Task{IResult{TOut}}"/> representing the asynchronous operation, yielding a new <see cref="IResult{TOut}"/>
        /// with the provided value if successful, or the original failure information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="valueProvider"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> MapAsync<TOut>(this IResult result, Func<Task<TOut>> valueProvider)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(valueProvider);

            return result.IsSuccess
                ? Result<TOut>.Success(value: await valueProvider().ConfigureAwait(false), messages: result.Messages)
                : Result<TOut>.Failure(value: default, result.Errors);
        }

        #endregion

        #region Bind Extensions (Result Chaining / Monadic FlatMap)

        /// <summary>
        /// Chains a generic <see cref="IResult{TIn}"/> into a new synchronous operation producing an <see cref="IResult{TOut}"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">A synchronous function to invoke on the success value, returning a new <see cref="IResult{TOut}"/>.</param>
        /// <returns>The result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static IResult<TOut> Bind<TIn, TOut>(this IResult<TIn> result, Func<TIn, IResult<TOut>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? binder(result.Value!)
                : Result<TOut>.Failure(default, result.Errors);
        }

        /// <summary>
        /// Asynchronously chains a generic <see cref="IResult{TIn}"/> into a new asynchronous operation producing an <see cref="IResult{TOut}"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">An asynchronous function to invoke on the success value, returning a <see cref="Task{IResult{TOut}}"/>.</param>
        /// <returns>A <see cref="Task{IResult{TOut}}"/> representing the asynchronous operation, yielding the result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> BindAsync<TIn, TOut>(this IResult<TIn> result, Func<TIn, Task<IResult<TOut>>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? await binder(result.Value!).ConfigureAwait(false)
                : Result<TOut>.Failure(default, result.Errors);
        }

        /// <summary>
        /// Synchronously chains a generic <see cref="IResult{TIn}"/> into a non-generic <see cref="IResult"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">A synchronous function to invoke on the success value, returning a non-generic <see cref="IResult"/>.</param>
        /// <returns>The result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static IResult Bind<TIn>(this IResult<TIn> result, Func<TIn, IResult> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? binder(result.Value!)
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Asynchronously chains a generic <see cref="IResult{TIn}"/> into a new asynchronous operation producing a non-generic <see cref="IResult"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">An asynchronous function to invoke on the success value, returning a <see cref="Task{IResult}"/>.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing the asynchronous operation, yielding the result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static async Task<IResult> BindAsync<TIn>(this IResult<TIn> result, Func<TIn, Task<IResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? await binder(result.Value!).ConfigureAwait(false)
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Synchronously chains a non-generic <see cref="IResult"/> into another non-generic <see cref="IResult"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <param name="result">The original result.</param>
        /// <param name="binder">A synchronous function to invoke if successful, returning a new <see cref="IResult"/>.</param>
        /// <returns>The result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static IResult Bind(this IResult result, Func<IResult> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? binder()
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Asynchronously chains a non-generic <see cref="IResult"/> into another asynchronous operation producing a non-generic <see cref="IResult"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <param name="result">The original result.</param>
        /// <param name="binder">An asynchronous function to invoke if successful, returning a <see cref="Task{IResult}"/>.</param>
        /// <returns>A <see cref="Task{IResult}"/> representing the asynchronous operation, yielding the result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static async Task<IResult> BindAsync(this IResult result, Func<Task<IResult>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? await binder().ConfigureAwait(false)
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Synchronously chains a non-generic <see cref="IResult"/> into a generic <see cref="IResult{TOut}"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">A synchronous function to invoke if successful, returning a new <see cref="IResult{TOut}"/>.</param>
        /// <returns>The result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static IResult<TOut> Bind<TOut>(this IResult result, Func<IResult<TOut>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? binder()
                : Result<TOut>.Failure(default, result.Errors);
        }

        /// <summary>
        /// Asynchronously chains a non-generic <see cref="IResult"/> into an asynchronous operation producing a generic <see cref="IResult{TOut}"/>.
        /// If the original result is a failure, the failure propagates without executing the binding function.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="binder">An asynchronous function to invoke if successful, returning a <see cref="Task{IResult{TOut}}"/>.</param>
        /// <returns>A <see cref="Task{IResult{TOut}}"/> representing the asynchronous operation, yielding the result from <paramref name="binder"/> if successful, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> BindAsync<TOut>(this IResult result, Func<Task<IResult<TOut>>> binder)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(binder);

            return result.IsSuccess
                ? await binder().ConfigureAwait(false)
                : Result<TOut>.Failure(default, result.Errors);
        }

        #endregion

        #region Match Extensions (Consumption and Branching)

        /// <summary>
        /// Executes a success action if the generic <see cref="IResult{TValue}"/> is successful,
        /// or a failure action if it is a failure. Returns a value based on the outcome.
        /// This is typically used at the end of a fluent chain to consume the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the match operation.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The function to execute if the result is successful, receiving the success value.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving the list of errors.</param>
        /// <returns>The result of either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static TResult Match<TValue, TResult>(
            this IResult<TValue> result,
            Func<TValue, TResult> onSuccess,
            Func<IReadOnlyList<ErrorInfo>, TResult> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            return result.IsSuccess
                ? onSuccess(result.Value!)
                : onFailure(result.Errors);
        }

        /// <summary>
        /// Asynchronously executes a success function if the generic <see cref="IResult{TValue}"/> is successful,
        /// or a failure function if it is a failure. Returns a task yielding a value based on the outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the match operation.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The asynchronous function to execute if the result is successful, receiving the success value.</param>
        /// <param name="onFailure">The asynchronous function to execute if the result is a failure, receiving the list of errors.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, yielding the result of either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static async Task<TResult> MatchAsync<TValue, TResult>(
            this IResult<TValue> result,
            Func<TValue, Task<TResult>> onSuccess,
            Func<IReadOnlyList<ErrorInfo>, Task<TResult>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            return result.IsSuccess
                ? await onSuccess(result.Value!).ConfigureAwait(false)
                : await onFailure(result.Errors).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a success action if the non-generic <see cref="IResult"/> is successful,
        /// or a failure action if it is a failure. Returns a value based on the outcome.
        /// This is typically used at the end of a fluent chain to consume the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the match operation.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving the list of errors.</param>
        /// <returns>The result of either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static TResult Match<TResult>(
            this IResult result,
            Func<TResult> onSuccess,
            Func<IReadOnlyList<ErrorInfo>, TResult> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            return result.IsSuccess
                ? onSuccess()
                : onFailure(result.Errors);
        }

        /// <summary>
        /// Asynchronously executes a success function if the non-generic <see cref="IResult"/> is successful,
        /// or a failure function if it is a failure. Returns a task yielding a value based on the outcome.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the match operation.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The asynchronous function to execute if the result is successful.</param>
        /// <param name="onFailure">The asynchronous function to execute if the result is a failure, receiving the list of errors.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, yielding the result of either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static async Task<TResult> MatchAsync<TResult>(
            this IResult result,
            Func<Task<TResult>> onSuccess,
            Func<IReadOnlyList<ErrorInfo>, Task<TResult>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);

            return result.IsSuccess
                ? await onSuccess().ConfigureAwait(false)
                : await onFailure(result.Errors).ConfigureAwait(false);
        }

        #endregion
    }
}
