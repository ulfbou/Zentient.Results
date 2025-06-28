# Zentient.Results Changelog

---
Here's the rewritten 0.4.2 section of the Changelog, omitting any mention of `Zentient.Results.AspNetCore` as a breaking change:

---
## Version 0.4.2 (Current release)

This is a patch update focused on further enhancing the robustness, consistency, and internal clarity of `Result` object creation and error handling. This release refines how errors and messages are managed within the core `Result` types and streamlines factory methods.

### New Features

* **Refined Error and Message Handling in `Result` and `Result<T>`:**
    * The `Result` constructor and `Failure` factory methods for both `Result` (non-generic) and `Result<T>` have been improved. They now reliably extract messages from `ErrorInfo` objects and exceptions (in `FromException`) and populate the `Messages` collection, ensuring that all relevant error details are available.
    * Standardized the use of `default(T)` across `Result<T>` failure factory methods for improved type consistency and clarity.

* **Streamlined Result Factory Methods:**
    * The static factory methods for common failure scenarios (e.g., `NotFound`, `Unauthorized`, `Validation`, `Conflict`, `RequestTimeout`, `Gone`, etc.) on both `Result` and `Result<T>` have been refactored. They now leverage `Zentient.Results.Constants.ResultStatusConstants` for standardized default messages and error codes, promoting greater consistency across various failure types.

* **Enhanced Test Coverage and Validation:**
    * Updated existing tests and added new ones in `ResultTTests.cs` and `ResultTests.cs` to explicitly verify the correct propagation of error messages and the handling of null/empty error collections.
    * Refined assertions in tests, specifically for `ErrorInfo.Metadata`, to correctly expect an empty collection rather than a null value.
    * Adjusted expected exception types in tests to accurately reflect current argument validation behavior (e.g., `ArgumentNullException` for null error collections when expected).
    * Removed `FromHttpStatusCode` tests from `ResultStatusTests.cs` to align with the removal of the corresponding method from `ResultStatus`.

* **Codebase Clean-up and Consistency:**
    * Removed unnecessary Byte Order Mark (BOM) characters from `Result.cs`, `Result{T}.cs`, `ResultStatusTests.cs`, `ResultTTests.cs`, and `ResultTests.cs` for minor encoding detail improvements.
    * Made minor adjustments to the implicit conversion test in `ResultTTests.cs` for explicit clarity.
    * Updated the version in `Zentient.Results.csproj` to 0.4.2.

### Breaking Changes

* **No new breaking changes** in this patch release. `Zentient.Results 0.4.2` is compatible with `Zentient.Results 0.4.1` and `0.4.0` at the core public API level. Any breaking changes from 0.4.0 (e.g., `Result` and `Result<T>` transitioning to sealed classes, `IResult.Error` renamed to `ErrorMessage`) still apply if you are upgrading from versions prior to 0.4.0.

### Bug Fixes

* Corrected handling of null or empty messages in `Result` and `Result<T>` factory methods to ensure the `Messages` collection accurately reflects available information.
* Fixed a potential issue where `_firstError` lazy initialization might return null inappropriately if `ErrorInfo` did not contain a message or code.

## Version 0.4.1 (Previous Release)

This is a patch release focused on enhancing the integration and compatibility of `Zentient.Results` with other libraries within the Zentient ecosystem, specifically addressing internal visibility concerns.

### New Features

* **Expanded Internal Visibility (`InternalsVisibleTo`):**
    * To streamline development and enable advanced scenarios, `Zentient.Results` now explicitly exposes its internal types and members to a set of designated friend assemblies.
    * This includes key projects such as `Zentient.Endpoints`, `Zentient.Telemetry`, and their associated test and extension libraries.
    * This ensures that these consuming libraries can access necessary internal components (e.g., for testing or deep integration) when `Zentient.Results` is consumed as a NuGet package, thereby resolving compilation issues and integration friction encountered with version 0.4.0.

### Breaking Changes

* **No new breaking changes** in this patch release. `Zentient.Results 0.4.1` is fully compatible with `Zentient.Results 0.4.0` at the public API level. Any breaking changes from 0.4.0 (e.g., `Result` and `Result<T>` transitioning to sealed classes, `IResult.Error` renamed to `ErrorMessage`) still apply if you are upgrading from versions prior to 0.4.0.

### Addressed Integration/Compatibility Issues

* Mitigated internal visibility challenges that prevented direct access to internal types and members from `Zentient.Endpoints`, `Zentient.Telemetry`, and related projects when `Zentient.Results 0.4.0` was referenced as a NuGet package.

---

## Version 0.4.0

This release introduces significant enhancements to the Zentient.Results library, focusing on improved error handling, expanded `ResultStatus` definitions, and more robust JSON serialization.

### New Features

* **Expanded `ResultStatus` Definitions**: The `Zentient.Results.Constants.ResultStatusConstants` now includes a comprehensive set of HTTP status codes (1xx, 3xx, and additional 4xx/5xx codes) and their corresponding descriptions. This provides a richer set of predefined statuses for more granular result reporting.
* **Enhanced `ErrorInfo` Class**:
    * `ErrorInfo` is now a `sealed class` instead of a `readonly struct`, allowing for better extensibility and reference equality checks.
    * Added a `Detail` property for more descriptive error messages.
    * Introduced a `Metadata` dictionary (`IImmutableDictionary<string, object?>`) to store additional, arbitrary data related to an error. This enables richer error context, especially for exceptions (e.g., stack traces, source information).
    * The `FromException` static method on `ErrorInfo` now automatically captures `ExceptionMessage`, `ExceptionStackTrace`, `ExceptionSource`, and `ExceptionType` into the `Metadata` dictionary.
    * New constructors and static factory methods have been added for easier creation of common `ErrorInfo` types (`General`, `Validation`, `NotFound`, `Authentication`, `Authorization`, `Conflict`).
    * Existing `ErrorInfo` factory methods (`Validation`, `FromException`) have been updated to ensure consistency with the new immutability requirements for `Metadata`.
* **Refactored `Result` and `Result<T>` Classes**:
    * Both `Result` (non-generic) and `Result<T>` (generic) are now `class` types instead of `structs`. This aligns with common patterns for complex types and enables potential future inheritance scenarios.
    * **Improved JSON Serialization**: A new `ResultJsonConverter` and its internal generic/non-generic implementations (`ResultGenericJsonConverter<TValue>` and `ResultNonGenericJsonConverter`) are introduced to handle JSON serialization and deserialization of `Result` and `Result<T>` objects more robustly. This converter now explicitly serializes `IsSuccess`, `IsFailure`, `Status`, `Messages`, `Errors`, and `ErrorMessage`.
    * The `ErrorMessage` property on `IResult` (and consequently `Result` and `Result<T>`) is now explicitly `ErrorMessage` (was `Error`), providing clearer naming.
    * Added new static factory methods for common HTTP success and failure statuses directly on `Result` and `Result<T>`: `Accepted`, `RequestTimeout`, `Gone`, `PreconditionFailed`, `TooManyRequests`, `NotImplemented`, `ServiceUnavailable`.
    * `Result<T>` now includes a `MapError` method, allowing transformation of the error list when the result is a `Failure`.
* **Enhanced Extension Methods**:
    * `ResultCreationHelpersExtensions`: Added `AsResult` overloads and `AsNoContent<T>`.
    * `ResultSideEffectExtensions`: `OnSuccess` and `OnFailure` extensions for both `IResult` and `IResult<T>` are now available.
    * `ResultTransformationExtensions`: Introduced new `Bind` overloads for `IResult` and `IResult<T>` to simplify chaining operations.
    * `ResultTryExtensions`: New `Try` extension methods for `Action` and `Func<T>` to wrap potentially exception-throwing code into a `Result` or `Result<T>`.
* **Extended ResultStatuses and Thread-Safe Caching**:
    * The `ResultStatuses` class now includes a more comprehensive set of pre-defined HTTP-aligned statuses, such as `TooManyRequests`, `ImATeapot`, and others, providing richer options for status reporting.
    * The internal caching mechanism for `IResultStatus` instances within `ResultStatuses` now uses a `ConcurrentDictionary`, ensuring thread-safe retrieval and addition of custom statuses.
* **Robust JSON Serialization with System.Text.Json**:
    * A dedicated `ResultJsonConverter` (implementing `JsonConverterFactory`) now provides robust and explicit serialization and deserialization for `Result` and `Result<T>` types, `ErrorInfo`, and `IResultStatus`.
    * This ensures consistent and predictable JSON payloads, making integration with APIs and messaging systems seamless. Properties like `isSuccess`, `isfailure`, `status`, `messages`, `errors`, `errorMessage`, and `value` (for `Result<T>`) are explicitly serialized, adhering to `JsonSerializerOptions.PropertyNamingPolicy`.
* **Enhanced Code Quality and Reliability**:
    * The internal project directory structure has been reorganized for improved clarity and maintainability.
    * Test assertions have been simplified and aligned with the new `ErrorInfo` structure, reflecting the enhanced immutability.
    * General refinements and optimizations have been applied across the codebase, including updates to helper classes like `Guard.cs`.
    * **Comprehensive Test Coverage**: This release has been rigorously tested and **passes all 166 unit and integration tests**, ensuring high reliability and predictability.

### Breaking Changes

* `ErrorInfo` is now a `sealed class` instead of a `readonly struct`. This means `ErrorInfo` instances are now reference types.
* `Result` and `Result<T>` are now `class` types instead of `structs`. This changes their default behavior regarding nullability and assignment (they are now reference types).
* The `Error` property on `IResult` (and its implementations) has been renamed to `ErrorMessage` for clarity.
* The internal `Constants` class has been split and reorganized into `Zentient.Results.Constants.ErrorCodes`, `Zentient.Results.Constants.JsonConstants`, `Zentient.Results.Constants.MetadataKeys`, and `Zentient.Results.Constants.ResultStatusConstants` for better organization and clarity.
* Default constructors for `ErrorInfo` have changed signatures to align with the new property structure.
* JSON serialization behavior for `Result` and `Result<T>` is now explicitly managed by `ResultJsonConverter`, which might alter the exact JSON output structure compared to previous versions (e.g., inclusion of `IsSuccess`, `IsFailure` properties).

### Bug Fixes

* The JSON serialization logic has been completely rewritten to address potential issues and provide more consistent and robust serialization/deserialization of `Result` and `Result<T>` objects, including nested `ErrorInfo` and `IResultStatus` instances.
* Addressed potential null reference issues in `Result<T>.Failure` factory methods when `errors` collection was null or empty.
* Corrected the `_firstError` lazy initialization in `Result<T>` to better handle the case where `Errors` collection might be empty.

---

## Version 0.3.0

This release significantly enhances error handling capabilities, strengthens JSON serialization, and introduces robust integration points for ASP.NET Core's Problem Details standard, along with other internal refinements.

### ✨ Features & Enhancements

* **Improved ErrorInfo Structure:**
    * Added a `Detail` property to `ErrorInfo` for more descriptive error information, aligning with the Problem Details specification.
    * Introduced an `Extensions` dictionary to `ErrorInfo`, allowing for custom, non-standard error properties to be included, enhancing flexibility and diagnostic capabilities.
    * Modified `ErrorInfo` constructors to accept these new properties, providing more comprehensive error creation options.
    * Switched `InnerErrors` to `IReadOnlyList<ErrorInfo>` and `Extensions` to `IReadOnlyDictionary<string, object?>` for immutability and consistency.
    * The `InternalServerError` member within the `ErrorCategory` enum now explicitly includes the `[EnumMember(Value = "internal_server_error")]` attribute for consistent serialization.
    * **New `ProblemDetails` Error Category**: Introduced `ErrorCategory.ProblemDetails` to explicitly classify errors stemming from RFC 7807 Problem Details.

* **Enhanced JSON Serialization and Deserialization:**
    * The `ResultJsonConverter` has been substantially improved to provide a more comprehensive and robust JSON handling experience.
    * **Explicit Property Serialization**: The `Write` methods for both `Result` and `Result<T>` now explicitly ensure that **all critical public properties**, including `IsSuccess`, `IsFailure`, `Status`, `Messages` (if present), `Errors` (if present), and `Value` (for `Result<T>`), are serialized to the JSON output. This guarantees a complete and consistent representation of the result state in JSON.
    * **Robust Deserialization**: The `Read` methods within `ResultNonGenericJsonConverter` and `ResultGenericJsonConverter<TValue>` have been strengthened. If the `Status` property is missing or cannot be deserialized, the converter will now **default to `ResultStatuses.Error`** and inject a descriptive `ErrorInfo` indicating a deserialization issue. This prevents malformed results and provides immediate feedback on data inconsistencies, making deserialization more fault-tolerant.

* **New HTTP Status Code for Request Timeout:**
    * Added `RequestTimeout` (HTTP 408) to `Constants.Code` and `Constants.Description`, along with a corresponding `ResultStatuses.RequestTimeout` static instance, providing more granular control over timeout-related error reporting.

* **Refined Exception Handling:**
    * Updated `FromException` factory methods in `Result` and `Result<T>` to correctly pass the `Exception` object as `data` to the `ErrorInfo` constructor.

* **ASP.NET Core Problem Details Integration:**
    * Added an `internal static IResult Problem(ProblemDetails problemDetails)` factory method to `Result` within a `#if PROBLEM_DETAILS` conditional compilation block. This enables direct conversion of ASP.NET Core `ProblemDetails` instances (including `ValidationProblemDetails` and its `Errors` dictionary) into `Zentient.Results.IResult` failures, streamlining mapping external problem details into your application's result flow.
    * Introduced `ResultStatus.FromHttpStatusCode(int statusCode)` to simplify creating or retrieving `IResultStatus` from an integer HTTP status code.

* **General Code Improvements:**
    * Corrected the `ResultStatuses.Forbidden` description to accurately reflect "Forbidden."
    * Adjusted `Result<T>.Failure` factory method to ensure `errors` are not null or empty before creating a failure result.
    * Updated the project description in `Zentient.Results.csproj` to indicate compatibility with **.NET 6-9**.
    * Streamlined JSON serialization logic within `ResultJsonConverter` for `ErrorInfo` to pass `data` and `innerErrors` explicitly.

---

## Version 0.2.0

This release introduces significant enhancements to the `Zentient.Results` library, focusing on improved consistency, robustness, and internal clarity. Key updates include renaming `DefaultResultStatus` for better alignment, refining the `ErrorInfo` message retrieval, and enhancing the `Result<T>` structure for more explicit value handling.

### ✨ Features & Enhancements

* **Renamed `DefaultResultStatus` to `ResultStatus`**: The concrete implementation of `IResultStatus` has been renamed from `DefaultResultStatus` to `ResultStatus`. This change simplifies the naming convention and makes it more intuitive for users, reflecting its primary role as the standard status object.
* **Refined `ErrorInfo` Message Retrieval in `Result<T>`**: The lazy evaluation of the `Error` property in `Result<T>` has been enhanced. It now prioritizes `ErrorInfo.Message`, then `ErrorInfo.Code`, and finally `ErrorInfo.Data.ToString()` for a more robust and predictable retrieval of the primary error message when multiple pieces of information are available.
* **Explicit `Value` Handling in `Result<T>.Failure` Factory Methods**: All `Result<T>.Failure` factory methods now explicitly accept a `T? value` parameter. This allows for scenarios where a default or partially constructed value might still be useful even in a failed result, providing more flexibility in error handling pipelines.
* **Introduced `ResultException` for `ThrowIfFailure`**: A new custom exception type, `ResultException`, has been introduced. This exception is specifically thrown by the `ThrowIfFailure()` extension method (which now lives in `ResultExtensions`), providing a structured way to expose `IReadOnlyList<ErrorInfo>` when a result is forcibly unwrapped on failure.
* **Improved Error Handling in `Result<T>.Failure` Overloads**:
    * The `Failure(T? value, IEnumerable<ErrorInfo> errors, IResultStatus status)` method now includes explicit `Guard` clauses to prevent `null` or empty `errors` collections, enhancing robustness and preventing invalid result states.
    * The error message for `ArgumentException` now correctly states "Error messages cannot be null or empty."
* **Enhanced `ResultStatus` Equality**: `ResultStatus` now explicitly implements `IEquatable<ResultStatus>`, ensuring correct and consistent value equality comparisons for result statuses throughout the application.
* **Added `Result<T>.NoContent()` Factory Method**: A new static factory method `Result<T>.NoContent(string? message = null)` has been added for generic results. This provides a clear and consistent way to indicate a successful operation with no content, aligning with HTTP 204 No Content semantics, particularly useful when `T` is a value type or when an empty value is ambiguous.

---

## Version 0.1.0 (Initial Release)

This is the inaugural release of the Zentient.Results library, providing a robust and flexible framework for handling operation outcomes with explicit success and failure states.

### ✨ Features

* **`ErrorCategory` Enum**: Introduced a comprehensive set of predefined error categories to classify different types of operation failures, including `General`, `Validation`, `Authentication`, `Authorization`, `NotFound`, `Conflict`, `Exception`, `Network`, `Database`, `Timeout`, `Security`, and `Request`.
* **`ErrorInfo` Struct**: A lightweight, immutable structure for detailed error representation, including `Category`, `Code`, `Message`, optional `Data`, and `InnerErrors` for nested error scenarios.
    * Provides a static `Aggregate` method for creating validation-related `ErrorInfo` instances.
* **`IResultStatus` Interface & `DefaultResultStatus` Struct**: Defines a standard for representing the status of an operation with a `Code` (intended to align with HTTP status codes) and `Description`.
    * `DefaultResultStatus` provides a concrete implementation and a `Custom` static method for creating custom statuses.
* **`IResult` Interface**: Defines the fundamental contract for an operation result, providing properties to check for `IsSuccess` or `IsFailure`, access `Errors`, `Messages`, a single `Error` message, and the `Status` of the operation.
* **`Result<T>` Struct**: A generic, immutable struct representing an operation result that can carry a `Value` upon success.
    * **Success Factory Methods**: `Success`, `Created`, and `NoContent` for common successful outcomes.
    * **Failure Factory Methods**: `Failure` (for single or multiple errors), `Validation`, `FromException`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, and `InternalError` for various failure scenarios.
    * **Monadic Operations**: Includes `Map`, `Bind`, `Tap`, `OnSuccess`, and `OnFailure` for functional-style composition and error handling.
    * **Value Retrieval**: `GetValueOrThrow()`, `GetValueOrThrow(string message)`, `GetValueOrThrow(Func<Exception> exceptionFactory)`, and `GetValueOrDefault(T fallback)` for safe or explicit value access.
    * **Pattern Matching**: `Match` method allows for elegant handling of success and failure branches.
    * **Implicit Conversion**: Supports implicit conversion from `T` to `Result<T>` for concise success returns.
* **`Result` Struct (Non-Generic)**: A non-generic version of `Result` for operations that do not return a specific value, but still convey success or failure.
    * Provides similar success and failure factory methods as `Result<T>`.
    * Includes implicit conversion from `ErrorInfo` for direct error returns.
* **`ResultJsonConverter`**: Custom `JsonConverterFactory` to enable proper JSON serialization and deserialization of `Result` and `Result<T>` types, ensuring `IsSuccess`, `IsFailure`, `Status`, `Messages`, `Errors`, and `Value` (for `Result<T>`) are correctly handled.
* **`ResultStatuses` Class**: A static class providing a comprehensive collection of predefined `IResultStatus` instances, largely mapping to standard HTTP status codes (e.g., `Success` (200), `Created` (201), `BadRequest` (400), `Unauthorized` (401), `NotFound` (404), `InternalError` (500)).
* **`ResultStatusExtensions`**: An extension method `ToHttpStatusCode()` for converting an `IResultStatus` to its integer HTTP status code.

**Last Updated:** 2025-06-23 **Version:** 0.4.2
