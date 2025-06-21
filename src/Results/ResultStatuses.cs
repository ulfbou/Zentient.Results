// <copyright file="ResultStatuses.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Results.Constants;

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Zentient.Results
{
    /// <summary>
    /// Provides a comprehensive set of common, predefined result statuses, typically mapping to HTTP status codes.
    /// This class serves as a central registry for standard and custom result statuses.
    /// </summary>
    public static class ResultStatuses
    {
        private static readonly ConcurrentDictionary<int, IResultStatus> _statuses;

        // --- 2xx Success Statuses ---
        public static readonly IResultStatus Ok = new ResultStatus(ResultStatusConstants.Code.Ok, ResultStatusConstants.Description.Ok);
        public static readonly IResultStatus Success = Ok;
        public static readonly IResultStatus Created = new ResultStatus(ResultStatusConstants.Code.Created, ResultStatusConstants.Description.Created);
        public static readonly IResultStatus Accepted = new ResultStatus(ResultStatusConstants.Code.Accepted, ResultStatusConstants.Description.Accepted);
        public static readonly IResultStatus NoContent = new ResultStatus(ResultStatusConstants.Code.NoContent, ResultStatusConstants.Description.NoContent);

        // --- 3xx Redirection Statuses ---
        public static readonly IResultStatus MultipleChoices = new ResultStatus(ResultStatusConstants.Code.MultipleChoices, ResultStatusConstants.Description.MultipleChoices);
        public static readonly IResultStatus MovedPermanently = new ResultStatus(ResultStatusConstants.Code.MovedPermanently, ResultStatusConstants.Description.MovedPermanently);
        public static readonly IResultStatus Found = new ResultStatus(ResultStatusConstants.Code.Found, ResultStatusConstants.Description.Found);
        public static readonly IResultStatus SeeOther = new ResultStatus(ResultStatusConstants.Code.SeeOther, ResultStatusConstants.Description.SeeOther);
        public static readonly IResultStatus NotModified = new ResultStatus(ResultStatusConstants.Code.NotModified, ResultStatusConstants.Description.NotModified);
        public static readonly IResultStatus TemporaryRedirect = new ResultStatus(ResultStatusConstants.Code.TemporaryRedirect, ResultStatusConstants.Description.TemporaryRedirect);
        public static readonly IResultStatus PermanentRedirect = new ResultStatus(ResultStatusConstants.Code.PermanentRedirect, ResultStatusConstants.Description.PermanentRedirect);

        // --- 4xx Client Error Statuses ---
        public static readonly IResultStatus BadRequest = new ResultStatus(ResultStatusConstants.Code.BadRequest, ResultStatusConstants.Description.BadRequest);
        public static readonly IResultStatus Unauthorized = new ResultStatus(ResultStatusConstants.Code.Unauthorized, ResultStatusConstants.Description.Unauthorized);
        public static readonly IResultStatus PaymentRequired = new ResultStatus(ResultStatusConstants.Code.PaymentRequired, ResultStatusConstants.Description.PaymentRequired); // Consistent new ResultStatus
        public static readonly IResultStatus Forbidden = new ResultStatus(ResultStatusConstants.Code.Forbidden, ResultStatusConstants.Description.Forbidden);
        public static readonly IResultStatus NotFound = new ResultStatus(ResultStatusConstants.Code.NotFound, ResultStatusConstants.Description.NotFound);
        public static readonly IResultStatus MethodNotAllowed = new ResultStatus(ResultStatusConstants.Code.MethodNotAllowed, ResultStatusConstants.Description.MethodNotAllowed);
        public static readonly IResultStatus NotAcceptable = new ResultStatus(ResultStatusConstants.Code.NotAcceptable, ResultStatusConstants.Description.NotAcceptable);
        public static readonly IResultStatus RequestTimeout = new ResultStatus(ResultStatusConstants.Code.RequestTimeout, ResultStatusConstants.Description.RequestTimeout);
        public static readonly IResultStatus Conflict = new ResultStatus(ResultStatusConstants.Code.Conflict, ResultStatusConstants.Description.Conflict);
        public static readonly IResultStatus Gone = new ResultStatus(ResultStatusConstants.Code.Gone, ResultStatusConstants.Description.Gone);
        public static readonly IResultStatus LengthRequired = new ResultStatus(ResultStatusConstants.Code.LengthRequired, ResultStatusConstants.Description.LengthRequired);
        public static readonly IResultStatus PreconditionFailed = new ResultStatus(ResultStatusConstants.Code.PreconditionFailed, ResultStatusConstants.Description.PreconditionFailed);
        public static readonly IResultStatus PayloadTooLarge = new ResultStatus(ResultStatusConstants.Code.PayloadTooLarge, ResultStatusConstants.Description.PayloadTooLarge);
        public static readonly IResultStatus UriTooLong = new ResultStatus(ResultStatusConstants.Code.UriTooLong, ResultStatusConstants.Description.UriTooLong);
        public static readonly IResultStatus UnsupportedMediaType = new ResultStatus(ResultStatusConstants.Code.UnsupportedMediaType, ResultStatusConstants.Description.UnsupportedMediaType);
        public static readonly IResultStatus RangeNotSatisfiable = new ResultStatus(ResultStatusConstants.Code.RangeNotSatisfiable, ResultStatusConstants.Description.RangeNotSatisfiable);
        public static readonly IResultStatus ExpectationFailed = new ResultStatus(ResultStatusConstants.Code.ExpectationFailed, ResultStatusConstants.Description.ExpectationFailed);
        public static readonly IResultStatus ImATeapot = new ResultStatus(ResultStatusConstants.Code.ImATeapot, ResultStatusConstants.Description.ImATeapot);
        public static readonly IResultStatus UnprocessableEntity = new ResultStatus(ResultStatusConstants.Code.UnprocessableEntity, ResultStatusConstants.Description.UnprocessableEntity);
        public static readonly IResultStatus Locked = new ResultStatus(ResultStatusConstants.Code.Locked, ResultStatusConstants.Description.Locked);
        public static readonly IResultStatus FailedDependency = new ResultStatus(ResultStatusConstants.Code.FailedDependency, ResultStatusConstants.Description.FailedDependency);
        public static readonly IResultStatus TooEarly = new ResultStatus(ResultStatusConstants.Code.TooEarly, ResultStatusConstants.Description.TooEarly);
        public static readonly IResultStatus UpgradeRequired = new ResultStatus(ResultStatusConstants.Code.UpgradeRequired, ResultStatusConstants.Description.UpgradeRequired);
        public static readonly IResultStatus PreconditionRequired = new ResultStatus(ResultStatusConstants.Code.PreconditionRequired, ResultStatusConstants.Description.PreconditionRequired);
        public static readonly IResultStatus TooManyRequests = new ResultStatus(ResultStatusConstants.Code.TooManyRequests, ResultStatusConstants.Description.TooManyRequests);
        public static readonly IResultStatus RequestHeaderFieldsTooLarge = new ResultStatus(ResultStatusConstants.Code.RequestHeaderFieldsTooLarge, ResultStatusConstants.Description.RequestHeaderFieldsTooLarge);
        public static readonly IResultStatus UnavailableForLegalReasons = new ResultStatus(ResultStatusConstants.Code.UnavailableForLegalReasons, ResultStatusConstants.Description.UnavailableForLegalReasons);

        // --- 5xx Server Error Statuses ---
        public static readonly IResultStatus Error = new ResultStatus(ResultStatusConstants.Code.InternalServerError, ResultStatusConstants.Description.InternalServerError);
        public static readonly IResultStatus InternalServerError = Error; // Alias for clarity
        public static readonly IResultStatus NotImplemented = new ResultStatus(ResultStatusConstants.Code.NotImplemented, ResultStatusConstants.Description.NotImplemented);
        public static readonly IResultStatus BadGateway = new ResultStatus(ResultStatusConstants.Code.BadGateway, ResultStatusConstants.Description.BadGateway);
        public static readonly IResultStatus ServiceUnavailable = new ResultStatus(ResultStatusConstants.Code.ServiceUnavailable, ResultStatusConstants.Description.ServiceUnavailable);
        public static readonly IResultStatus GatewayTimeout = new ResultStatus(ResultStatusConstants.Code.GatewayTimeout, ResultStatusConstants.Description.GatewayTimeout);
        public static readonly IResultStatus HttpVersionNotSupported = new ResultStatus(ResultStatusConstants.Code.HttpVersionNotSupported, ResultStatusConstants.Description.HttpVersionNotSupported);
        public static readonly IResultStatus VariantAlsoNegotiates = new ResultStatus(ResultStatusConstants.Code.VariantAlsoNegotiates, ResultStatusConstants.Description.VariantAlsoNegotiates);
        public static readonly IResultStatus InsufficientStorage = new ResultStatus(ResultStatusConstants.Code.InsufficientStorage, ResultStatusConstants.Description.InsufficientStorage);
        public static readonly IResultStatus LoopDetected = new ResultStatus(ResultStatusConstants.Code.LoopDetected, ResultStatusConstants.Description.LoopDetected);
        public static readonly IResultStatus NotExtended = new ResultStatus(ResultStatusConstants.Code.NotExtended, ResultStatusConstants.Description.NotExtended);
        public static readonly IResultStatus NetworkAuthenticationRequired = new ResultStatus(ResultStatusConstants.Code.NetworkAuthenticationRequired, ResultStatusConstants.Description.NetworkAuthenticationRequired);

        /// <summary>
        /// Initializes the <see cref="ResultStatuses"/> class by populating the internal dictionary
        /// with all predefined result status instances.
        /// </summary>
        static ResultStatuses()
        {
            _statuses = new ConcurrentDictionary<int, IResultStatus>();

            _statuses.TryAdd(Ok.Code, Ok);
            _statuses.TryAdd(Created.Code, Created);
            _statuses.TryAdd(Accepted.Code, Accepted);
            _statuses.TryAdd(NoContent.Code, NoContent);

            _statuses.TryAdd(MultipleChoices.Code, MultipleChoices);
            _statuses.TryAdd(MovedPermanently.Code, MovedPermanently);
            _statuses.TryAdd(Found.Code, Found);
            _statuses.TryAdd(SeeOther.Code, SeeOther);
            _statuses.TryAdd(NotModified.Code, NotModified);
            _statuses.TryAdd(TemporaryRedirect.Code, TemporaryRedirect);
            _statuses.TryAdd(PermanentRedirect.Code, PermanentRedirect);

            _statuses.TryAdd(BadRequest.Code, BadRequest);
            _statuses.TryAdd(Unauthorized.Code, Unauthorized);
            _statuses.TryAdd(PaymentRequired.Code, PaymentRequired);
            _statuses.TryAdd(Forbidden.Code, Forbidden);
            _statuses.TryAdd(NotFound.Code, NotFound);
            _statuses.TryAdd(MethodNotAllowed.Code, MethodNotAllowed);
            _statuses.TryAdd(NotAcceptable.Code, NotAcceptable);
            _statuses.TryAdd(RequestTimeout.Code, RequestTimeout);
            _statuses.TryAdd(Conflict.Code, Conflict);
            _statuses.TryAdd(Gone.Code, Gone);
            _statuses.TryAdd(LengthRequired.Code, LengthRequired);
            _statuses.TryAdd(PreconditionFailed.Code, PreconditionFailed);
            _statuses.TryAdd(PayloadTooLarge.Code, PayloadTooLarge);
            _statuses.TryAdd(UriTooLong.Code, UriTooLong);
            _statuses.TryAdd(UnsupportedMediaType.Code, UnsupportedMediaType);
            _statuses.TryAdd(RangeNotSatisfiable.Code, RangeNotSatisfiable);
            _statuses.TryAdd(ExpectationFailed.Code, ExpectationFailed);
            _statuses.TryAdd(ImATeapot.Code, ImATeapot);
            _statuses.TryAdd(UnprocessableEntity.Code, UnprocessableEntity);
            _statuses.TryAdd(Locked.Code, Locked);
            _statuses.TryAdd(FailedDependency.Code, FailedDependency);
            _statuses.TryAdd(TooEarly.Code, TooEarly);
            _statuses.TryAdd(UpgradeRequired.Code, UpgradeRequired);
            _statuses.TryAdd(PreconditionRequired.Code, PreconditionRequired);
            _statuses.TryAdd(TooManyRequests.Code, TooManyRequests);
            _statuses.TryAdd(RequestHeaderFieldsTooLarge.Code, RequestHeaderFieldsTooLarge);
            _statuses.TryAdd(UnavailableForLegalReasons.Code, UnavailableForLegalReasons);

            _statuses.TryAdd(Error.Code, Error);
            _statuses.TryAdd(NotImplemented.Code, NotImplemented);
            _statuses.TryAdd(BadGateway.Code, BadGateway);
            _statuses.TryAdd(ServiceUnavailable.Code, ServiceUnavailable);
            _statuses.TryAdd(GatewayTimeout.Code, GatewayTimeout);
            _statuses.TryAdd(HttpVersionNotSupported.Code, HttpVersionNotSupported);
            _statuses.TryAdd(VariantAlsoNegotiates.Code, VariantAlsoNegotiates);
            _statuses.TryAdd(InsufficientStorage.Code, InsufficientStorage);
            _statuses.TryAdd(LoopDetected.Code, LoopDetected);
            _statuses.TryAdd(NotExtended.Code, NotExtended);
            _statuses.TryAdd(NetworkAuthenticationRequired.Code, NetworkAuthenticationRequired);
        }

        /// <summary>
        /// Retrieves a result status by its code. If the status is one of the predefined ones,
        /// the cached instance is returned. Otherwise, a new <see cref="CustomResultStatus"/>
        /// is created and returned (and potentially cached).
        /// </summary>
        /// <param name="code">The status code (e.g., HTTP status code).</param>
        /// <param name="description">An optional description for custom statuses. If null, a default description is used.</param>
        /// <returns>An instance of <see cref="IResultStatus"/> matching the code.</returns>
        public static IResultStatus GetStatus(int code, string? description = null)
        {
            return _statuses.GetOrAdd(code, _ => new CustomResultStatus(code, description ?? $"Custom Status: {code}"));
        }

        /// <summary>
        /// Represents a standard, immutable implementation of <see cref="IResultStatus"/>.
        /// This class is used for predefined statuses.
        /// </summary>
        private sealed class ResultStatus : IResultStatus
        {
            public int Code { get; }
            public string Description { get; }

            public ResultStatus(int code, string description)
            {
                Code = code;
                Description = description;
            }

            public override string ToString() => $"[{Code}] {Description}";
        }

        /// <summary>
        /// Represents a custom implementation of <see cref="IResultStatus"/> for user-defined status codes and descriptions.
        /// This class is used when <see cref="GetStatus"/> is called with a non-predefined code.
        /// </summary>
        private sealed class CustomResultStatus : IResultStatus
        {
            public int Code { get; }
            public string Description { get; }

            public CustomResultStatus(int code, string description)
            {
                Code = code;
                Description = description;
            }

            public override string ToString() => $"[{Code}] {Description} (Custom)";
        }
    }
}
