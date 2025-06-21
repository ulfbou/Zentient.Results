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
    /// to facilitate fluent, expressive, and consistent functional-style error handling and result manipulation.
    /// </summary>
    public static class ResultTransformationExtensions
    {
        #region Transformation and Binding (Map / Bind / Then)

        /// <summary>
        /// Transforms the success value of a generic <see cref="IResult{TIn}"/> to a new type.
        /// If the result is failure, the errors and status propagate unchanged.
        /// </summary>
        /// <typeparam name="TIn">Type of input value.</typeparam>
        /// <typeparam name="TOut">Type of output value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="selector">Function to map from <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.</param>
        /// <returns>A new <see cref="IResult{TOut}"/> with the transformed value or failure info.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="selector"/> is <c>null</c>.</exception>
        public static IResult<TOut> Map<TIn, TOut>(this IResult<TIn> result, Func<TIn, TOut> selector)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(selector);

            return result.IsSuccess
                ? Result<TOut>.Success(selector(result.Value!))
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Asynchronously chains a success result into a new asynchronous operation producing an <see cref="IResult{TOut}"/>.
        /// If the original result is failure, the failure propagates without executing the function.
        /// </summary>
        /// <typeparam name="TIn">Input value type.</typeparam>
        /// <typeparam name="TOut">Output value type.</typeparam>
        /// <param name="result">The original result.</param>
        /// <param name="next">Async function to invoke on success value.</param>
        /// <returns>A task yielding the chained <see cref="IResult{TOut}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="next"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> Bind<TIn, TOut>(this IResult<TIn> result, Func<TIn, Task<IResult<TOut>>> next)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(next);

            return result.IsFailure
                ? Result<TOut>.Failure(default, result.Errors, result.Status)
                : await next(result.Value!).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously chains a success non-generic <see cref="IResult"/> into a new asynchronous operation producing an <see cref="IResult"/>.
        /// If the original result is failure, the failure propagates without executing the function.
        /// </summary>
        /// <param name="result">The original result.</param>
        /// <param name="next">Async function to invoke if success.</param>
        /// <returns>A task yielding the chained <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="next"/> is <c>null</c>.</exception>
        public static async Task<IResult> Bind(this IResult result, Func<Task<IResult>> next)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(next);

            return result.IsFailure
                ? Result.Failure(result.Errors, result.Status)
                : await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Chains a generic <see cref="IResult{TIn}"/> into a non-generic <see cref="IResult"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <typeparam name="TIn">Input value type.</typeparam>
        /// <param name="result">Original result.</param>
        /// <param name="func">Function to execute if success, returning non-generic result.</param>
        /// <returns>Result from <paramref name="func"/> or original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult Then<TIn>(this IResult<TIn> result, Func<TIn, IResult> func)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(func);

            return result.IsFailure
                ? Result.Failure(result.Errors, result.Status)
                : func(result.Value!);
        }

        /// <summary>
        /// Synchronously chains a generic <see cref="IResult{TIn}"/> into a non-generic <see cref="IResult"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <typeparam name="TIn">Input value type.</typeparam>
        /// <param name="result">Original result.</param>
        /// <param name="binder">Function to execute if success, returning non-generic result.</param>
        /// <returns>Result from <paramref name="binder"/> or original failure.</returns>
        public static IResult Bind<TIn>(this IResult<TIn> result, Func<TIn, IResult> binder)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return result.IsSuccess
                ? binder(result.Value!)
                : Result.Failure(result.Errors, result.Status);
        }

        /// <summary>
        /// Synchronously chains a non-generic <see cref="IResult"/> into another non-generic <see cref="IResult"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <param name="result">Original result.</param>
        /// <param name="binder">Function to execute if success, returning <see cref="IResult"/>.</param>
        /// <returns>Result from <paramref name="binder"/> or original failure.</returns>
        public static IResult Bind(this IResult result, Func<IResult> binder)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return result.IsSuccess
                ? binder()
                : Result.Failure(result.Errors, result.Status);
        }

        /// <summary>
        /// Synchronously chains a non-generic <see cref="IResult"/> into a generic <see cref="IResult{TOut}"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <typeparam name="TOut">Output value type.</typeparam>
        /// <param name="result">Original result.</param>
        /// <param name="binder">Function to execute if success, returning <see cref="IResult{TOut}"/>.</param>
        /// <returns>Result from <paramref name="binder"/> or original failure.</returns>
        public static IResult<TOut> Bind<TOut>(this IResult result, Func<IResult<TOut>> binder)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return result.IsSuccess
                ? binder()
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Chains a generic <see cref="IResult{TIn}"/> into another generic <see cref="IResult{TOut}"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <typeparam name="TIn">Input value type.</typeparam>
        /// <typeparam name="TOut">Output value type.</typeparam>
        /// <param name="result">Original result.</param>
        /// <param name="func">Function to execute if success, returning <see cref="IResult{TOut}"/>.</param>
        /// <returns>Result from <paramref name="func"/> or original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult<TOut> Then<TIn, TOut>(this IResult<TIn> result, Func<TIn, IResult<TOut>> func)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(func);

            return result.IsFailure
                ? Result<TOut>.Failure(default, result.Errors, result.Status)
                : func(result.Value!);
        }

        /// <summary>
        /// Chains a non-generic <see cref="IResult"/> into another non-generic <see cref="IResult"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <param name="result">Original result.</param>
        /// <param name="func">Function to execute if success, returning <see cref="IResult"/>.</param>
        /// <returns>Result from <paramref name="func"/> or original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult Then(this IResult result, Func<IResult> func)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(func);

            return result.IsFailure
                ? Result.Failure(result.Errors, result.Status)
                : func();
        }

        /// <summary>
        /// Chains a non-generic <see cref="IResult"/> into a generic <see cref="IResult{TOut}"/>.
        /// If the original is failure, failure propagates unchanged.
        /// </summary>
        /// <typeparam name="TOut">Output value type.</typeparam>
        /// <param name="result">Original result.</param>
        /// <param name="func">Function to execute if success, returning <see cref="IResult{TOut}"/>.</param>
        /// <returns>Result from <paramref name="func"/> or original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult<TOut> Then<TOut>(this IResult result, Func<IResult<TOut>> func)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(func);

            return result.IsFailure
                ? Result<TOut>.Failure(default, result.Errors, result.Status)
                : func();
        }

        #endregion
    }
}
