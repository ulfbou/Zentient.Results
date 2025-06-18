// <copyright file="ResultExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Provides extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate common operations and improve readability in fluent chains.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Determines if a non-generic <see cref="IResult"/> represents a successful operation.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool IsSuccess(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsSuccess;
        }

        /// <summary>
        /// Determines if a generic <see cref="IResult{TValue}"/> represents a successful operation.
        /// This overload exists primarily for explicit type inference when chaining.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference and consistency.")]
        public static bool IsSuccess<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsSuccess;
        }

        /// <summary>
        /// Determines if a non-generic <see cref="IResult"/> represents a failed operation.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool IsFailure(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsFailure;
        }

        /// <summary>
        /// Determines if a generic <see cref="IResult{TValue}"/> represents a failed operation.
        /// This overload exists primarily for explicit type inference when chaining.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference and consistency.")]
        public static bool IsFailure<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsFailure;
        }

        /// <summary>
        /// Checks if a failed <see cref="IResult"/> contains an error of a specific category.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="category">The error category to check for.</param>
        /// <returns><c>true</c> if the result is a failure and contains at least one error of the specified category; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool HasErrorCategory(this IResult result, ErrorCategory category)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsFailure && result.Errors.Any(e => e.Category == category);
        }

        /// <summary>
        /// Checks if a failed <see cref="IResult"/> contains an error with a specific error code.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="errorCode">The error code to check for (case-sensitive).</param>
        /// <returns><c>true</c> if the result is a failure and contains at least one error with the specified code; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="errorCode"/> is <c>null</c>.</exception>
        public static bool HasErrorCode(this IResult result, string errorCode)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(errorCode, nameof(errorCode));
            return result.IsFailure && result.Errors.Any(e => e.Code == errorCode);
        }

        /// <summary>
        /// Executes a specified action if the non-generic <see cref="IResult"/> is successful, then returns the original result.
        /// This is useful for side-effects like logging without changing the result's state.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="onSuccess">The action to execute if the result is successful.</param>
        /// <returns>The original <see cref="IResult"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult OnSuccess(this IResult result, Action onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));

            if (result.IsSuccess)
            {
                onSuccess();
            }
            return result;
        }

        /// <summary>
        /// Executes a specified action if the generic <see cref="IResult{TValue}"/> is successful, passing the value, then returns the original result.
        /// This is useful for side-effects like logging without changing the result's state.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <param name="onSuccess">The action to execute if the result is successful.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnSuccess<TValue>(this IResult<TValue> result, Action<TValue> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));

            if (result.IsSuccess)
            {
                onSuccess(result.Value!);
            }
            return result;
        }

        /// <summary>
        /// Executes a specified action if the non-generic <see cref="IResult"/> is a failure, passing the list of errors, then returns the original result.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="onFailure">The action to execute if the result is a failure.</param>
        /// <returns>The original <see cref="IResult"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult OnFailure(this IResult result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }
            return result;
        }

        /// <summary>
        /// Executes a specified action if the generic <see cref="IResult{TValue}"/> is a failure, passing the list of errors, then returns the original result.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <param name="onFailure">The action to execute if the result is a failure.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnFailure<TValue>(this IResult<TValue> result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }
            return result;
        }

        /// <summary>
        /// Transforms the success value of a generic <see cref="IResult{TIn}"/> to a new type using a selector function.
        /// If the current result is a failure, it propagates the failure with the original errors and status.
        /// </summary>
        /// <typeparam name="TIn">The input type of the result's value.</typeparam>
        /// <typeparam name="TOut">The output type of the new result's value.</typeparam>
        /// <param name="result">The <see cref="IResult{TIn}"/> instance.</param>
        /// <param name="selector">A function to transform the success value.</param>
        /// <returns>A new <see cref="IResult{TOut}"/> containing the transformed value or the original errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="selector"/> is <c>null</c>.</exception>
        public static IResult<TOut> Map<TIn, TOut>(this IResult<TIn> result, Func<TIn, TOut> selector)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(selector, nameof(selector));

            return result.IsSuccess
                ? Result<TOut>.Success(selector(result.Value!))
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Chains the current generic <see cref="IResult{TIn}"/> with another asynchronous operation that returns an <see cref="IResult{TOut}"/>.
        /// If the current result is a success, the 'next' function is executed. If a failure, the failure is propagated
        /// with the original errors and status.
        /// </summary>
        /// <typeparam name="TIn">The input type of the result's value.</typeparam>
        /// <typeparam name="TOut">The output type of the new result's value.</typeparam>
        /// <param name="result">The <see cref="IResult{TIn}"/> instance.</param>
        /// <param name="next">An asynchronous function that takes the current success value and returns a new <see cref="Task{IResult{TOut}}"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result of the 'next' function or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="next"/> is <c>null</c>.</exception>
        public static async Task<IResult<TOut>> Bind<TIn, TOut>(this IResult<TIn> result, Func<TIn, Task<IResult<TOut>>> next)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return result.IsSuccess
                ? await next(result.Value!).ConfigureAwait(false)
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Chains the current non-generic <see cref="IResult"/> with another asynchronous operation that returns an <see cref="IResult"/>.
        /// If the current result is a success, the 'next' function is executed. If a failure, the failure is propagated
        /// with the original errors.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="next">An asynchronous function that returns a new <see cref="Task{IResult}"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result of the 'next' function or the original failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="next"/> is <c>null</c>.</exception>
        public static async Task<IResult> Bind(this IResult result, Func<Task<IResult>> next)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            return result.IsSuccess
                ? await next().ConfigureAwait(false)
                : Result.Failure(result.Errors, result.Status);
        }

        /// <summary>
        /// Unwraps the value from a successful <see cref="IResult{TValue}"/>.
        /// If the result is a failure, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <returns>The success value of the result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        public static TValue Unwrap<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsFailure)
            {
                throw new InvalidOperationException("Attempted to unwrap a failed result. Errors: " + string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            return result.Value!;
        }

        /// <summary>
        /// Gets the value from a successful <see cref="IResult{TValue}"/>, or returns a specified default value if the result is a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}"/> instance.</param>
        /// <param name="defaultValue">The value to return if the result is a failure or its value is null.</param>
        /// <returns>The success value if available, otherwise the <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result, TValue defaultValue)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return (result.IsSuccess ? result.Value : defaultValue)!; // Cast to TValue to satisfy compiler, nullability handled by logic.
        }

        /// <summary>
        /// Converts a non-generic <see cref="IResult"/> into an <see cref="IResult{bool}"/>,
        /// where success is mapped to <c>true</c> and failure to <c>false</c>, preserving errors and status.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <returns>A new <see cref="IResult{bool}"/> representing the success or failure state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IResult<bool> ToBoolResult(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsSuccess ? Result<bool>.Success(true, result.Status, result.Messages) : Result<bool>.Failure(false, result.Errors, result.Status); // Preserve messages
        }

        /// <summary>
        /// Concatenates all error messages from a failed <see cref="IResult"/> into a single string.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <param name="separator">The string to use as a separator between messages. Defaults to "; ".</param>
        /// <returns>A single string containing all error messages, or an empty string if no errors are present.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string ToErrorString(this IResult result, string separator = "; ")
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsFailure && result.Errors != null
                ? string.Join(separator, result.Errors.Select(e => e.Message))
                : string.Empty;
        }

        /// <summary>
        /// Gets the message of the first error from a failed <see cref="IResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance.</param>
        /// <returns>The message of the first error, or <c>null</c> if the result is successful or has no errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string? FirstErrorMessage(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return result.IsFailure && result.Errors != null && result.Errors.Any()
                ? result.Errors[0].Message
                : null;
        }

        /// <summary>
        /// Chains a non-generic <see cref="IResult"/> operation after a generic <see cref="IResult{TIn}"/> operation.
        /// If the first result is a success, the 'func' is executed. If a failure, the failure is propagated.
        /// </summary>
        /// <typeparam name="TIn">The input type of the previous result's value.</typeparam>
        /// <param name="result">The <see cref="IResult{TIn}"/> instance.</param>
        /// <param name="func">A function that takes the previous success value and returns a new <see cref="IResult"/>.</param>
        /// <returns>The result of the 'func' operation or the propagated failure from the first result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult Then<TIn>(this IResult<TIn> result, Func<TIn, IResult> func)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            return result.IsSuccess
                ? func(result.Value!)
                : Result.Failure(result.Errors, result.Status);
        }

        /// <summary>
        /// Chains a generic <see cref="IResult{TOut}"/> operation after a generic <see cref="IResult{TIn}"/> operation.
        /// If the first result is a success, the 'func' is executed. If a failure, the failure is propagated.
        /// </summary>
        /// <typeparam name="TIn">The input type of the previous result's value.</typeparam>
        /// <typeparam name="TOut">The output type of the new result's value.</typeparam>
        /// <param name="result">The <see cref="IResult{TIn}"/> instance.</param>
        /// <param name="func">A function that takes the previous success value and returns a new <see cref="IResult{TOut}"/>.</param>
        /// <returns>The result of the 'func' operation or the propagated failure from the first result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult<TOut> Then<TIn, TOut>(this IResult<TIn> result, Func<TIn, IResult<TOut>> func)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            return result.IsSuccess
                ? func(result.Value!)
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Chains a non-generic <see cref="IResult"/> operation after another non-generic <see cref="IResult"/> operation.
        /// If the first result is a success, the 'func' is executed. If a failure, the failure is propagated.
        /// </summary>
        /// <param name="result">The initial <see cref="IResult"/> instance.</param>
        /// <param name="func">A function that returns a new <see cref="IResult"/>.</param>
        /// <returns>The result of the 'func' operation or the propagated failure from the first result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult Then(this IResult result, Func<IResult> func)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            return result.IsSuccess
                ? func()
                : Result.Failure(result.Errors, result.Status);
        }

        /// <summary>
        /// Chains a generic <see cref="IResult{TOut}"/> operation after a non-generic <see cref="IResult"/> operation.
        /// If the first result is a success, the 'func' is executed. If a failure, the failure is propagated.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the new result.</typeparam>
        /// <param name="result">The initial <see cref="IResult"/> instance.</param>
        /// <param name="func">A function that returns a new <see cref="IResult{TOut}"/>.</param>
        /// <returns>The result of the 'func' operation or the propagated failure from the first result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public static IResult<TOut> Then<TOut>(this IResult result, Func<IResult<TOut>> func)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            ArgumentNullException.ThrowIfNull(func, nameof(func));
            return result.IsSuccess
                ? func()
                : Result<TOut>.Failure(default, result.Errors, result.Status);
        }

        /// <summary>
        /// Throws a <see cref="ResultException"/> if the <see cref="IResult"/> is a failure.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance to check.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="ResultException">Thrown if the result is a failure, containing all associated errors.</exception>
        public static void ThrowIfFailure(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsFailure)
            {
                throw new ResultException(result.Errors);
            }
        }
    }
}
