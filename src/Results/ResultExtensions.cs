using System.Net;
using System.Diagnostics.CodeAnalysis;

namespace Zentient.Results
{
    /// <summary>Provides common extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>.</summary>
    public static class ResultExtensions
    {
        /// <summary>Checks if the result is a success result with no value.</summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <returns><see langword="true"/> if the result is a success; otherwise, <see langword="false"/>.</returns>
        public static bool IsSuccess(this IResult result) => result.IsSuccess;

        /// <summary>Checks if the result is a success result with a value.</summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <returns><see langword="true"/> if the result is a success with a value; otherwise, <see langword="false"/>.</returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference.")]
        public static bool IsSuccess<TValue>(this IResult<TValue> result) => result.IsSuccess;

        /// <summary>Checks if the result is a failure result.</summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <returns><see langword="true"/> if the result is a failure; otherwise, <see langword="false"/>.</returns>
        public static bool IsFailure(this IResult result) => result.IsFailure;

        /// <summary>Checks if the result is a failure result.</summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <returns><see langword="true"/> if the result is a failure; otherwise, <see langword="false"/>.</returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference.")]
        public static bool IsFailure<TValue>(this IResult<TValue> result) => result.IsFailure;

        /// <summary>
        /// Checks if the result is a failure and contains an error with the specified category.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="category">The error category to check for.</param>
        /// <returns><see langword="true"/> if the result is a failure and contains an error of the specified category; otherwise, <see langword="false"/>.</returns>
        public static bool HasErrorCategory(this IResult result, ErrorCategory category)
            => result.IsFailure && result.Errors.Any(e => e.Category == category);

        /// <summary>
        /// Checks if the result is a failure and contains an error with the specified code.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="errorCode">The error code to check for.</param>
        /// <returns><see langword="true"/> if the result is a failure and contains an error with the specified code; otherwise, <see langword="false"/>.</returns>
        public static bool HasErrorCode(this IResult result, string errorCode)
            => result.IsFailure && result.Errors.Any(e => e.Code == errorCode);

        /// <summary>Executes an action if the result is a success.</summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="onSuccess">The action to execute on success.</param>
        /// <returns>The original <see cref="IResult" /> instance for chaining.</returns>
        public static IResult OnSuccess(this IResult result, Action onSuccess)
        {
            if (result.IsSuccess)
            {
                onSuccess();
            }
            return result;
        }

        /// <summary>Executes an action if the result is a success, providing the value.</summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <param name="onSuccess">The action to execute on success with the value.</param>
        /// <returns>The original <see cref="IResult{TValue}" /> instance for chaining.</returns>
        public static IResult<TValue> OnSuccess<TValue>(this IResult<TValue> result, Action<TValue> onSuccess)
        {
            if (result.IsSuccess)
            {
                onSuccess(result.Value!);
            }

            return result;
        }

        /// <summary>Executes an action if the result is a failure.</summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="onFailure">The action to execute on failure, providing the errors.</param>
        /// <returns>The original <see cref="IResult" /> instance for chaining.</returns>
        public static IResult OnFailure(this IResult result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        /// <summary>Executes an action if the result is a failure.</summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <param name="onFailure">The action to execute on failure, providing the errors.</param>
        /// <returns>The original <see cref="IResult{TValue}" /> instance for chaining.</returns>
        public static IResult<TValue> OnFailure<TValue>(this IResult<TValue> result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        /// <summary>Transforms a success result to a new type, or propagates failure. (Map/Select)</summary>
        /// <typeparam name="TIn">The input value type.</typeparam>
        /// <typeparam name="TOut">The output value type.</typeparam>
        /// <param name="result">The IResult&lt;TIn&gt; instance.</param>
        /// <param name="selector">The function to transform the value if successful.</param>
        /// <returns>A new <see cref="IResult{TOut}" /> representing the transformed result or the original failure.</returns>
        public static IResult<TOut> Map<TIn, TOut>(this IResult<TIn> result, Func<TIn, TOut> selector)
        {
            return result.IsSuccess
                ? Result<TOut>.Success(selector(result.Value!))
                : Result.Failure<TOut>(result.Errors);
        }

        /// <summary>
        /// Chains an asynchronous operation that returns an IResult&lt;TOut&gt;, propagating failure. (Bind/SelectMany)
        /// </summary>
        /// <typeparam name="TIn">The input value type.</typeparam>
        /// <typeparam name="TOut">The output value type.</typeparam>
        /// <param name="result">The IResult&lt;TIn&gt; instance.</param>
        /// <param name="next">The asynchronous function to execute if successful.</param>
        /// <returns>A new IResult&lt;TOut&gt; representing the chained result or the original failure.</returns>
        public static async Task<IResult<TOut>> Bind<TIn, TOut>(this IResult<TIn> result, Func<TIn, Task<IResult<TOut>>> next)
        {
            return result.IsSuccess
                ? await next(result.Value!)
                : Result.Failure<TOut>(result.Errors);
        }

        /// <summary>
        /// Chains an asynchronous operation that returns an IResult, propagating failure.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="next">The asynchronous function to execute if successful.</param>
        /// <returns>A new <see cref="IResult" /> representing the chained result or the original failure.</returns>
        public static async Task<IResult> Bind(this IResult result, Func<Task<IResult>> next)
        {
            return result.IsSuccess
                ? await next()
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Provides the value of a successful result, or throws an exception if it's a failure.
        /// Use sparingly and only when a failure is truly unrecoverable or indicates a programming error.
        /// </summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <returns>The value of the successful result.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        public static TValue Unwrap<TValue>(this IResult<TValue> result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException("Attempted to unwrap a failed result. Errors: " + string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            return result.Value!;
        }

        /// <summary>
        /// Provides the value of a successful result, or returns a default value if it's a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The <see cref="IResult{TValue}" /> instance.</param>
        /// <param name="defaultValue">The value to return if the result is a failure.</param>
        /// <returns>The value of the successful result or the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result, TValue defaultValue)
            => (result.IsSuccess ? result.Value : defaultValue)!;

        /// <summary>
        /// Converts a void <see cref="IResult" /> to an IResult<bool> indicating success.
        /// Useful for chaining void operations that should always return true on success.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <returns>An IResult<bool> where success is true or original failure.</returns>
        public static IResult<bool> ToBoolResult(this IResult result)
            => result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(false, result.Errors, result.Status);

        /// <summary>
        /// Converts the result's errors into a single string, typically for logging or display.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="separator">The string to use to separate error descriptions.</param>
        /// <returns>A concatenated string of all error descriptions, or an empty string if no errors.</returns>
        public static string ToErrorString(this IResult result, string separator = "; ")
        => result.IsFailure && result.Errors != null
            ? string.Join(separator, result.Errors.Select(e => e.Message))
            : string.Empty;

        /// <summary>
        /// Gets the first error's description if the result is a failure, otherwise null.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <returns>The description of the first error, or null.</returns>
        public static string? FirstErrorMessage(this IResult result)
        => result.IsFailure && result.Errors != null && result.Errors.Any()
            ? result.Errors[0].Message
            : null;

        /// <summary>
        /// Executes a function if the result is a success, returning a new IResult.
        /// Useful for conditional branching in fluent chains.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <param name="result">The IResult&lt;TIn&gt; instance.</param>
        /// <param name="func">The function to execute if successful, returning a new IResult.</param>
        /// <returns>The new <see cref="IResult" /> if successful, or the original failure.</returns>
        public static IResult Then<TIn>(this IResult<TIn> result, Func<TIn, IResult> func)
        {
            return result.IsSuccess
                ? func(result.Value!)
                : Result.Failure(result.Errors);
        }

        /// <summary>
        /// Executes a function if the result is a success, returning a new IResult&lt;TOut&gt;.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The IResult&lt;TIn&gt; instance.</param>
        /// <param name="func">The function to execute if successful, returning a new IResult&lt;TOut&gt;.</param>
        /// <returns>The new IResult&lt;TOut&gt; if successful, or the original failure.</returns>
        public static IResult<TOut> Then<TIn, TOut>(this IResult<TIn> result, Func<TIn, IResult<TOut>> func)
            => result.IsSuccess
                ? func(result.Value!)
                : Result<TOut>.Failure(default, result.Errors, result.Status);

        /// <summary>
        /// Executes a function if the result is a success (void), returning a new IResult.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="func">The function to execute if successful, returning a new IResult.</param>
        /// <returns>The new <see cref="IResult" /> if successful, or the original failure.</returns>
        public static IResult Then(this IResult result, Func<IResult> func)
            => result.IsSuccess
                ? func()
                : Result.Failure(result.Errors);

        /// <summary>
        /// Executes a function if the result is a success (void), returning a new IResult&lt;TOut&gt;.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        /// <param name="func">The function to execute if successful, returning a new IResult&lt;TOut&gt;.</param>
        /// <returns>The new IResult&lt;TOut&gt; if successful, or the original failure.</returns>
        public static IResult<TOut> Then<TOut>(this IResult result, Func<IResult<TOut>> func)
            => result.IsSuccess
                ? func()
                : Result<TOut>.Failure(default, result.Errors, result.Status);

        /// <summary>
        /// Throws an <see cref="ResultException"/> if the result is a failure.
        /// Useful at API boundaries for exception middleware.
        /// </summary>
        /// <param name="result">The <see cref="IResult" /> instance.</param>
        public static void ThrowIfFailure(this IResult result)
        {
            if (result.IsFailure)
            {
                throw new ResultException(result.Errors);
            }
        }

        /// <summary>
        /// Custom exception for throwing Zentient.Results errors.
        /// </summary>
        public class ResultException : Exception
        {
            public IReadOnlyList<ErrorInfo> Errors { get; }
            public ResultException(IReadOnlyList<ErrorInfo> errors)
                : base("One or more errors occurred: " + string.Join("; ", errors.Select(e => e.Message)))
            {
                Errors = errors;
            }
        }
    }
}
