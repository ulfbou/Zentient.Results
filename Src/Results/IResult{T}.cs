// <copyright file="IResult{T}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

#pragma warning disable CA1715 // Type names should not have the same name as a namespace
namespace Zentient.Results
{
    /// <summary>Represents the outcome of an operation that produces a value on success.</summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    public interface IResult<T> : IResult
    {
        /// <summary>
        /// Gets the value produced by the operation if it was successful.
        /// This property may be null even on success if the operation explicitly returns a null value.
        /// </summary>
        T? Value { get; }

        /// <summary>
        /// Gets the success value or throws an <see cref="InvalidOperationException"/> if the result is a failure.
        /// </summary>
        /// <returns>The success value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        T GetValueOrThrow();

        /// <summary>
        /// Gets the success value or throws an <see cref="InvalidOperationException"/> with a custom message if the result is a failure.
        /// </summary>
        /// <param name="message">The message for the <see cref="InvalidOperationException"/>.</param>
        /// <returns>The success value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        T GetValueOrThrow(string message);

        /// <summary>
        /// Gets the success value, or throws an exception produced by the provided factory if the result is a failure.
        /// </summary>
        /// <param name="exceptionFactory">A factory function to create the exception to throw.</param>
        /// <returns>The success value.</returns>
        /// <exception cref="Exception">The exception produced by the factory if the result is a failure.</exception>
        T GetValueOrThrow(Func<Exception> exceptionFactory);

        /// <summary>
        /// Gets the success value, or returns a fallback value if the result is a failure or the value is null.
        /// </summary>
        /// <param name="fallback">The value to return if the result is a failure or the success value is null.</param>
        /// <returns>The success value, or the fallback value.</returns>
        T GetValueOrDefault(T fallback);

        /// <summary>
        /// Transforms the success value of this result to a new type using a selector function.
        /// If the current result is a failure, it propagates the failure.
        /// </summary>
        /// <typeparam name="U">The new type of the success value.</typeparam>
        /// <param name="selector">A function to transform the success value.</param>
        /// <returns>A new <see cref="IResult{U}"/> containing the transformed value or the original errors.</returns>
        IResult<U> Map<U>(Func<T, U> selector);

        /// <summary>
        /// Chaines the current result with another operation that returns an <see cref="IResult{U}"/>.
        /// If the current result is a success, the binder function is executed. If a failure, the failure is propagated.
        /// </summary>
        /// <typeparam name="U">The type of the success value of the next operation.</typeparam>
        /// <param name="binder">A function that takes the current success value and returns a new <see cref="IResult{U}"/>.</param>
        /// <returns>The result of the binder function, or the original failure.</returns>
        IResult<U> Bind<U>(Func<T, IResult<U>> binder);

        /// <summary>
        /// Executes an action if the result is successful, then returns the original result.
        /// Useful for side-effects like logging or auditing without altering the result.
        /// </summary>
        /// <param name="onSuccess">The action to execute with the success value.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> Tap(Action<T> onSuccess);

        /// <summary>
        /// Executes an action if the result is successful.
        /// This method is an alias for <see cref="Tap(Action{T})"/>.
        /// </summary>
        /// <param name="action">The action to execute with the success value.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> OnSuccess(Action<T> action);

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        /// <param name="action">The action to execute with the list of errors.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action);

        /// <summary>
        /// Transforms the result into a single value by applying one of two functions
        /// based on whether the result is a success or a failure.
        /// </summary>
        /// <typeparam name="U">The type of the value to return.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        /// <returns>The result of either the success or failure function.</returns>
        U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure);
    }
}
