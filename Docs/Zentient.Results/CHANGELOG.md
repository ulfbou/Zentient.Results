# Changelog

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
