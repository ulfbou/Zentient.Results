using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Represents the outcome of an operation with a return value.
    /// It can be a success with a value and optional messages, or a failure with errors.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    [DataContract]
    [JsonConverter(typeof(ResultJsonConverter))]
    public readonly struct Result<T> : IResult<T>
    {
        /// <inheritdoc />
        [DataMember(Order = 1)]
        [JsonPropertyName("value")]
        public T? Value { get; }

        [DataMember(Order = 2)]
        [JsonIgnore]
        private readonly ErrorInfo[] _errors;

        [DataMember(Order = 3)]
        [JsonIgnore]
        private readonly string[] _messages;

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsSuccess =>
            (Status.Code >= 200 && Status.Code < 300) && _errors.Length == 0;

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        /// <inheritdoc />
        [JsonPropertyName("errors")]
        public IReadOnlyList<ErrorInfo> Errors => _errors;

        /// <inheritdoc />
        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages => _messages;

        [JsonIgnore]
        private readonly Lazy<string?> _firstError;

        /// <inheritdoc />
        [JsonIgnore]
        public string? Error => _firstError.Value;

        /// <inheritdoc />
        [DataMember(Order = 4)]
        [JsonPropertyName("status")]
        public IResultStatus Status { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> struct.
        /// </summary>
        /// <param name="value">The value to encapsulate (for success results).</param>
        /// <param name="status">The status of the result.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        [JsonConstructor]
        internal Result(
            T? value,
            IResultStatus status,
            IEnumerable<string>? messages = null,
            IEnumerable<ErrorInfo>? errors = null)
        {
            Value = value;
            Status = status;
            _errors = errors is null ? Array.Empty<ErrorInfo>() : errors as ErrorInfo[] ?? errors.ToArray();
            _messages = messages is null ? Array.Empty<string>() : messages as string[] ?? messages.ToArray();
            ErrorInfo[] errorsCopy = _errors;
            _firstError = new Lazy<string?>(() =>
            {
                ErrorInfo error = errorsCopy.FirstOrDefault();

                if (EqualityComparer<ErrorInfo>.Default.Equals(error, default))
                    return null;

                return error.Message ?? error.Code ?? error.Data?.ToString() ?? null;
            });
        }

        /// <summary>Creates a successful generic result.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        public static IResult<T> Success(T value, string? message = null) =>
            new Result<T>(value, ResultStatuses.Success, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with a "Created" status.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        public static IResult<T> Created(T value, string? message = null) =>
            new Result<T>(value, ResultStatuses.Created, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with a "No Content" status.</summary>
        /// <param name="message">An optional success message.</param>
        public static IResult<T> NoContent(string? message = null) =>
            new Result<T>(default, ResultStatuses.NoContent, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a generic failure result from a single error.</summary>
        /// <param name="value">The value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        public static IResult<T> Failure(T? value, ErrorInfo error, IResultStatus status) =>
            new Result<T>(value, status, null, new[] { error });

        /// <summary>Creates a generic failure result from a collection of errors.</summary>
        /// <param name="value">The value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult<T> Failure(T? value, IEnumerable<ErrorInfo> errors, IResultStatus status)
        {
            var arr = errors as ErrorInfo[] ?? errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors));
            if (arr.Length == 0)
            {
                throw new ArgumentException("Error messages cannot be null or empty.", nameof(errors));
            }

            return new Result<T>(value, status, null, arr);
        }

        /// <summary>Creates a generic failure result representing validation errors.</summary>
        /// <param name="errors">A collection of validation errors.</param>
        public static IResult<T> Validation(IEnumerable<ErrorInfo> errors) =>
            Failure(default, errors, ResultStatuses.UnprocessableEntity);

        /// <summary>
        /// Creates a generic failure result from an exception.
        /// </summary>
        /// <param name="value">The value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Error"/>.</param>
        public static IResult<T> FromException(T? value, Exception ex, IResultStatus? status = null)
            => Failure(value, new ErrorInfo(ErrorCategory.Exception, ex.GetType().Name, ex.Message, data: ex), status ?? ResultStatuses.Error);

        /// <summary>
        /// Allows implicit conversion from a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        public static implicit operator Result<T>(T value) =>
            (Result<T>)(value is null ? NoContent() : Success(value));

        /// <inheritdoc />
        public IResult<U> Map<U>(Func<T, U> selector) =>
            IsSuccess ? Result<U>.Success(selector(Value!)) : Result<U>.Failure(default, Errors, Status);

        /// <inheritdoc />
        public IResult<U> Bind<U>(Func<T, IResult<U>> binder) =>
            IsSuccess ? binder(Value!) : Result<U>.Failure(default, Errors, Status);

        /// <inheritdoc />
        public IResult<T> Tap(Action<T> onSuccess)
        {
            if (IsSuccess) onSuccess(Value!);
            return this;
        }

        /// <inheritdoc />
        public IResult<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess) action(Value!);
            return this;
        }

        /// <inheritdoc />
        public IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action)
        {
            if (IsFailure) action(Errors);
            return this;
        }

        /// <inheritdoc />
        public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) =>
            IsSuccess ? onSuccess(Value!) : onFailure(Errors);

        /// <inheritdoc />
        public T GetValueOrThrow() =>
            IsSuccess ? Value! : throw new InvalidOperationException(Error ?? "Result was not successful.");

        /// <inheritdoc />
        public T GetValueOrThrow(string message) =>
            IsSuccess ? Value! : throw new InvalidOperationException(message);

        /// <inheritdoc />
        public T GetValueOrThrow(Func<Exception> exceptionFactory) =>
            IsSuccess ? Value! : throw exceptionFactory();

        /// <inheritdoc />
        public T GetValueOrDefault(T fallback) => IsSuccess && Value is not null ? Value : fallback;

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"Result<{typeof(T).Name}>: Success ({Status}) | Value: {Value?.ToString() ?? "null"}{(Messages.Count > 0 ? $" | Message: {string.Join("; ", Messages)}" : "")}";
            }

            return $"Result<{typeof(T).Name}>: Failure ({Status}) | Error: {Error} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()))}";
        }
    }
}
