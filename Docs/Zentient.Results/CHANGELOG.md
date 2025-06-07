# Changelog

---

## Version 0.3.0 (Current release)

This release introduces focused refinements to the Zentient.Results library. Key updates include a serialization-specific attribute addition for `ErrorCategory`, significant improvements to the `ResultJsonConverter` for more robust and comprehensive JSON serialization and deserialization, and a minor correction to a predefined result status description. These changes aim to enhance the predictability and reliability of result handling, particularly in data exchange scenarios.

---

## Features & Enhancements

### Error Categorization Refinement

* **`ErrorCategory.InternalServerError` with `EnumMember` Attribute**: The `InternalServerError` member within the `ErrorCategory` enum now explicitly includes the `[EnumMember(Value = "internal_server_error")]` attribute. This ensures consistent and correct string representation during serialization, particularly when using data contracts or JSON serialization, improving interoperability.

### Enhanced JSON Serialization and Deserialization

The `ResultJsonConverter` has received substantial updates to provide a more comprehensive and robust JSON handling experience for `Result` and `Result<T>` types:

* **Explicit Property Serialization**: The `Write` methods for both `Result` and `Result<T>` now explicitly ensure that **all critical public properties**, including `IsSuccess`, `IsFailure`, `Status`, `Messages` (if present), `Errors` (if present), and `Value` (for `Result<T>`), are serialized to the JSON output. This guarantees a complete and consistent representation of the result state in JSON.
* **Improved Deserialization Robustness**: The `Read` methods within `ResultNonGenericJsonConverter` and `ResultGenericJsonConverter<TValue>` have been strengthened. If the `Status` property is missing or cannot be deserialized, the converter will now **default to `ResultStatuses.Error`** and inject a descriptive `ErrorInfo` indicating a deserialization issue. This prevents malformed results and provides immediate feedback on data inconsistencies, making deserialization more fault-tolerant.

### Result Status Description Correction

* **Corrected `ResultStatuses.Forbidden` Description**: The predefined `ResultStatuses.Forbidden` now accurately uses "Forbidden" as its description, aligning with the standard HTTP status code semantics, rather than the previous "Bad Request."

---

## Version 0.2.0 (Previous  Release)

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

This document serves as a foundational guide for understanding the architectural principles and components of `Zentient.Results`. It is intended to evolve alongside the library, reflecting changes and enhancements in future releases. For more detailed usage examples and API documentation, please refer to the [README.md](README.md) and the [Zentient.Results API documentation](https://github.com/ulfbou/Zentient.Results/wiki).

```
Last Updated: 2025-06-07
Version: 0.3.0
```
