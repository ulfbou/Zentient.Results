namespace Zentient.Results
{
    /// <summary>Provides a set of common, predefined result statuses.</summary>
    public static class ResultStatuses
    {
        private static readonly IDictionary<int, IResultStatus> _statuses;
        public static readonly IResultStatus Success = new ResultStatus(Constants.Code.Ok, Constants.Description.Ok);
        public static readonly IResultStatus Created = new ResultStatus(Constants.Code.Created, Constants.Description.Created);
        public static readonly IResultStatus Accepted = new ResultStatus(Constants.Code.Accepted, Constants.Description.Accepted);
        public static readonly IResultStatus NoContent = new ResultStatus(Constants.Code.NoContent, Constants.Description.NoContent);

        public static readonly IResultStatus BadRequest = new ResultStatus(Constants.Code.BadRequest, Constants.Description.BadRequest);
        public static readonly IResultStatus Unauthorized = new ResultStatus(Constants.Code.Unauthorized, Constants.Description.Unauthorized);
        public static readonly IResultStatus PaymentRequired = ResultStatus.Custom(Constants.Code.PaymentRequired, Constants.Description.PaymentRequired);
        public static readonly IResultStatus Forbidden = new ResultStatus(Constants.Code.Forbidden, Constants.Description.Forbidden);
        public static readonly IResultStatus NotFound = new ResultStatus(Constants.Code.NotFound, Constants.Description.NotFound);
        public static readonly IResultStatus MethodNotAllowed = new ResultStatus(Constants.Code.MethodNotAllowed, Constants.Description.MethodNotAllowed);
        public static readonly IResultStatus RequestTimeout = new ResultStatus(Constants.Code.RequestTimeout, Constants.Description.RequestTimeout);
        public static readonly IResultStatus Conflict = new ResultStatus(Constants.Code.Conflict, Constants.Description.Conflict);
        public static readonly IResultStatus Gone = new ResultStatus(Constants.Code.Gone, Constants.Description.Gone);
        public static readonly IResultStatus PreconditionFailed = new ResultStatus(Constants.Code.PreconditionFailed, Constants.Description.PreconditionFailed);
        public static readonly IResultStatus UnprocessableEntity = new ResultStatus(Constants.Code.UnprocessableEntity, Constants.Description.UnprocessableEntity);
        public static readonly IResultStatus TooManyRequests = new ResultStatus(Constants.Code.TooManyRequests, Constants.Description.TooManyRequests);

        public static readonly IResultStatus Error = new ResultStatus(Constants.Code.InternalServerError, Constants.Description.InternalServerError);
        public static readonly IResultStatus NotImplemented = new ResultStatus(Constants.Code.NotImplemented, Constants.Description.NotImplemented);
        public static readonly IResultStatus ServiceUnavailable = new ResultStatus(Constants.Code.ServiceUnavailable, Constants.Description.ServiceUnavailable);

        static ResultStatuses()
        {
            _statuses = new Dictionary<int, IResultStatus>
              {
                { Constants.Code.Ok, Success },
                { Constants.Code.Created, Created },
                { Constants.Code.Accepted, Accepted },
                { Constants.Code.NoContent, NoContent },
                { Constants.Code.BadRequest, BadRequest },
                { Constants.Code.Unauthorized, Unauthorized },
                { Constants.Code.PaymentRequired, PaymentRequired },
                { Constants.Code.Forbidden, Forbidden },
                { Constants.Code.NotFound, NotFound },
                { Constants.Code.MethodNotAllowed, MethodNotAllowed },
                { Constants.Code.RequestTimeout, RequestTimeout },
                { Constants.Code.Conflict, Conflict },
                { Constants.Code.Gone, Gone },
                { Constants.Code.PreconditionFailed, PreconditionFailed },
                { Constants.Code.UnprocessableEntity, UnprocessableEntity },
                { Constants.Code.TooManyRequests, TooManyRequests },
                { Constants.Code.InternalServerError, Error },
                { Constants.Code.NotImplemented, NotImplemented },
                { Constants.Code.ServiceUnavailable, ServiceUnavailable }
            };
        }

        /// <summary>Retrieves a custom result status by its code, creating it if it does not already exist.</summary>
        /// <param name="code">The custom status code.</param>
        /// <param name="description">An optional description for the custom status.</param>
        /// <returns>An instance of <see cref="IResultStatus"/> representing the custom status.</returns>
        public static IResultStatus GetStatus(int code, string? description = null)
        {
            if (!_statuses.ContainsKey(code))
            {
                _statuses.TryAdd(code, new CustomResultStatus(code, description ?? $"Custom code: {code}"));
            }

            return _statuses[code];
        }

        /// <summary>
        /// Represents a custom implementation of <see cref="IResultStatus"/> for user-defined status codes and descriptions.
        /// </summary>
        private sealed class CustomResultStatus : IResultStatus
        {
            /// <inheritdoc/>
            public int Code { get; }

            /// <inheritdoc/>
            public string Description { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomResultStatus"/> class with the specified code and description.
            /// </summary>
            /// <param name="code">The custom status code.</param>
            /// <param name="description">The description for the custom status.</param>
            public CustomResultStatus(int code, string description)
            {
                Code = code;
                Description = description;
            }
        }
    }
}
