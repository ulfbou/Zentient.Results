# Architecture of Zentient.Results

This document provides a comprehensive overview of the architectural design and underlying principles of the `Zentient.Results` library. It aims to clarify the structure, interactions, and rationale behind the components, offering insights for developers, architects, and maintainers.

---

## 1. Introduction and Purpose

`Zentient.Results` is a foundational .NET library designed to standardize the handling of operational outcomes. It moves beyond traditional exception-based control flow by introducing explicit, immutable result types that encapsulate either a successful value or a structured collection of errors. This architectural approach promotes cleaner code, enhances predictability, simplifies error propagation, and improves the overall observability of application behavior.

The primary goal of `Zentient.Results` is to provide a robust, composable, and extensible framework for communicating success and failure across various layers of a .NET application, aligning with modern software design principles such as Clean Architecture, CQRS, and Domain-Driven Design.

---

## 2. Architectural Goals

The architecture of `Zentient.Results` is guided by the following key objectives:

* **Predictable Outcomes:** Ensure that the success or failure of an operation is an explicit part of its contract, not an implicit side effect.
* **Structured Error Context:** Provide rich, categorized error information that's easily consumable by both code and humans.
* **Functional Composability:** Enable fluent chaining of operations, allowing for declarative and concise expression of business logic.
* **Immutability:** Guarantee that result objects are immutable after creation, enhancing thread safety and simplifying reasoning about state.
* **Extensibility:** Allow for customization of status codes, error categories, and result behaviors to fit diverse domain requirements.
* **Performance Efficiency:** Utilize lazy evaluation and consider performance implications of type choices to minimize overhead.
* **Minimal Dependencies:** Keep the core library lean to reduce footprint and avoid dependency conflicts.
* **Serialization Friendliness:** Facilitate seamless serialization and deserialization for integration with APIs and messaging systems.

---

## 3. Core Components

The `Zentient.Results` library is composed of several interconnected components, each serving a distinct purpose:

### 3.1. `IResult` and `Result` (Non-Generic)

* **Role:** Represents the outcome of an operation that doesn't return a specific value (e.g., a command that modifies state).
* **Key Properties:** `IsSuccess`, `IsFailure`, `Errors` (`IReadOnlyList<ErrorInfo>`), `Messages` (`IReadOnlyList<string>`), `ErrorMessage` (first error message), `Status` (`IResultStatus`).
* **Design:** `Result` is now a `sealed class`, designed for immutability through `readonly` fields and `init` accessors. It provides comprehensive static factory methods for creating success and various failure states (e.g., `Success()`, `Failure()`, `NotFound()`, `Unauthorized()`).

### 3.2. `IResult<T>` and `Result<T>` (Generic)

* **Role:** Represents the outcome of an operation that produces a specific value of type `T`.
* **Key Properties:** Inherits all properties from `IResult`, plus `Value` (`T?`).
* **Design:** `Result<T>` is also a `sealed class`, maintaining immutability through `readonly` fields and `init` accessors. It includes additional functional-style methods (`Map`, `Bind`, `OnSuccess`, `OnFailure`, `Match`) to compose operations on the contained `Value`. It provides static factory methods for success with a value, and various failure states, with `Ok` being an alias for `Success`.

### 3.3. `ErrorInfo` and `ErrorCategory`

* **Role:** `ErrorInfo` is a structured `sealed class` that encapsulates detailed information about a specific error. `ErrorCategory` is an `enum` providing predefined classifications for errors.
* **`ErrorInfo` Properties:** `Category` (`ErrorCategory`), `Code` (`string`), `Message` (`string`), `Detail` (`string?`), `Metadata` (`IImmutableDictionary<string, object?>`), `InnerErrors` (`IImmutableList<ErrorInfo>`).
* **Design:** `ErrorInfo` is designed to be comprehensive and extensible, allowing for rich context to be attached to failures. The `Detail` property offers a more specific explanation of the error occurrence. The `Metadata` dictionary (replacing `Data` and `Extensions` for consolidation) provides flexible inclusion of arbitrary key-value metadata, especially useful for exception details (e.g., stack trace, source). `InnerErrors` supports aggregation of multiple related errors. The `ErrorCategory` enum includes specific classifications like `InternalServerError`, `ProblemDetails`, and `RateLimit` for enhanced error categorization. `ErrorInfo` also provides static factory methods for common error types (e.g., `Validation`, `NotFound`, `FromException`).

### 3.4. `IResultStatus`, `ResultStatus`, and `ResultStatuses`

* **Role:** `IResultStatus` defines the contract for an operation's status. `ResultStatus` is the default `readonly struct` implementation, and `ResultStatuses` is a static class providing a centralized, thread-safe collection of commonly used `IResultStatus` instances.
* **`IResultStatus` Properties:** `Code` (`int`), `Description` (`string`).
* **Design:** `ResultStatus` is a `readonly struct` and implements `IEquatable<ResultStatus>`, ensuring value equality. `ResultStatuses` pre-populates a comprehensive set of HTTP-aligned statuses (e.g., 200 OK, 400 Bad Request, 408 Request Timeout, 429 Too Many Requests, 503 Service Unavailable) and uses a `ConcurrentDictionary` to allow for thread-safe retrieval and registration of custom statuses, promoting consistency. Includes a static `FromHttpStatusCode` method to create or retrieve `IResultStatus` instances from HTTP status codes.

### 3.5. Extension Methods (Grouped by Concern)

* **Role:** A comprehensive set of static extension methods for `IResult` and `IResult<T>`, now organized into several logical classes.
* **Design:** These extensions enhance the fluent API, simplify common result handling patterns, and provide utility for diagnostics and control flow. The logical grouping improves discoverability and maintainability.
    * `ResultConversionExtensions`: For converting between result types or extracting error strings.
    * `ResultCreationHelpersExtensions`: For simplified creation of results from basic types or exceptions.
    * `ResultExceptionThrowingExtensions`: For conditionally throwing `ResultException` on failure.
    * `ResultSideEffectExtensions`: For executing side effects (`OnSuccess`, `OnFailure`) without altering the result.
    * `ResultStatusCheckExtensions`: For querying the success/failure status and specific error categories/codes.
    * `ResultTransformationExtensions`: For chaining and transforming results (`Map`, `Bind`, `Then`). Includes async `Bind` overloads.
    * `ResultTryExtensions`: For wrapping potentially exception-throwing code into `Result` or `Result<T>`.
    * `ResultValueExtractionExtensions`: For unwrapping values or providing defaults.

### 3.6. `ResultJsonConverter`

* **Role:** A custom `System.Text.Json.JsonConverterFactory` that enables seamless and robust serialization and deserialization of `Result` and `Result<T>` types.
* **Design:** It dynamically creates specific converters (`ResultNonGenericJsonConverter`, `ResultGenericJsonConverter<TValue>`) for the non-generic and generic `Result` types. This ensures that `Result` objects can be correctly represented in JSON payloads. During serialization, it explicitly includes all relevant public properties: `IsSuccess`, `IsFailure`, `Status`, `Messages` (if present), `Errors` (if present), `ErrorMessage` (if present), and `Value` (for `Result<T>`). For deserialization, it provides robust logic, including a fallback to `ResultStatuses.Error` and the injection of a descriptive `ErrorInfo` if the status property is missing or cannot be deserialized, enhancing reliability and fault tolerance in data exchange. Property naming follows the `JsonSerializerOptions.PropertyNamingPolicy`.

### 3.7. `ResultException`

* **Role:** A custom `Exception` type specifically designed to be thrown when a `Result` indicates a failure and explicit exception throwing is desired (e.g., at the boundary of a system, after explicit decision to break the result chain).
* **Design:** It encapsulates a `IReadOnlyList<ErrorInfo>`, providing structured details about the failure instead of a generic message. Includes constructors that allow for specifying a custom message and/or an inner exception, aligning with standard .NET exception patterns.

---

## 4. Architectural Principles and Design Patterns

`Zentient.Results` embodies several key architectural principles and design patterns:

* **Monadic Pattern (Result Monad):** The `Map`, `Bind`, and `Then` methods on `Result` and `Result<T>` are direct implementations of monadic operations. This enables chaining of operations where the failure of any step short-circuits the entire sequence, propagating the error without explicit `if` checks at each stage.
    * `Map`: Transforms the successful value without changing the result's success/failure state.
    * `Bind`/`Then`: Allows chaining operations that *also* return a `Result`, effectively flattening nested results and facilitating sequential computations that might fail.
* **Value Object:** `ErrorInfo` and `ResultStatus` are designed as **value objects**. They are immutable, their equality is based on their constituent properties, and they encapsulate a concept rather than an identity.
* **Immutability:** All core result types (`Result`, `Result<T>`, `ErrorInfo`, `ResultStatus`) are designed as immutable types. `Result` and `Result<T>` are `sealed class`es with `readonly` fields and `init` properties, while `ErrorInfo` and `ResultStatus` are also immutable `sealed class` and `readonly struct` respectively. This design choice guarantees that once a result is created, its state cannot be altered, leading to:
    * **Thread Safety:** No shared mutable state, simplifying concurrent programming.
    * **Predictability:** Easier to reason about code as object state doesn't change unexpectedly.
    * **Referential Transparency:** Functions operating on `Result` objects can be considered "pure" if they only depend on their inputs and produce consistent outputs.
* **Separation of Concerns:**
    * **Outcome vs. Value:** `Result` and `Result<T>` focus solely on the outcome, while the actual data (`T`) or error details (`ErrorInfo`) are encapsulated within.
    * **Status vs. Error:** `IResultStatus` provides a high-level classification (e.g., HTTP status), while `ErrorInfo` provides granular, domain-specific details.
    * **Business Logic vs. Error Handling:** Business logic methods return `Result` objects, cleanly separating the domain operation from the mechanics of error propagation.
* **Composition over Inheritance:** The library favors composing results and their components (e.g., combining `ErrorInfo` instances) rather than relying on complex inheritance hierarchies for different result types.
* **Functional Programming Paradigms:** The fluent API and the monadic-inspired methods encourage a functional style, leading to more declarative, testable, and less error-prone code.

---

## 5. Integration Points

`Zentient.Results` is designed to integrate seamlessly into various layers and frameworks within a modern .NET application:

* **Application Service Layers:** Services can return `IResult<T>` from their methods, clearly signaling business outcomes (success or specific failures) without throwing exceptions.
* **ASP.NET Core (Controllers & Minimal APIs):** `IResult` and `IResult<T>` can be easily mapped to `IActionResult` types (e.g., `OkObjectResult`, `BadRequestObjectResult`, `NotFoundResult`, `UnprocessableEntityObjectResult`), providing consistent API responses. The `Zentient.Results.AspNetCore` library provides specific extensions for this mapping.
* **CQRS (e.g., MediatR):** Command and query handlers can return `IResult` or `IResult<T>`, allowing for explicit outcome handling within the pipeline, rather than relying on global exception handlers.
* **Logging and Observability:** The structured `ErrorInfo` (including `Detail` and `Metadata`) and `IResultStatus` provide rich context for logging, tracing (e.g., OpenTelemetry span tagging), and monitoring, enabling better diagnostics and incident response.
* **Domain-Driven Design (DDD):** Domain services and aggregates can return `IResult` to indicate the outcome of business operations, reinforcing domain invariants and explicit state transitions.
* **Resilience Patterns:** The categorized `ErrorInfo` and `IResultStatus` can be used to inform resilience strategies (e.g., circuit breakers, retries) by providing clear indications of transient vs. permanent failures.

---

## 6. Data Flow and Lifecycle

A typical data flow using `Zentient.Results` might look like this:

1.  **Domain/Infrastructure Layer:** A repository or external service call returns an `IResult<T>` (or `IResult`).
    * If successful, it contains the requested data.
    * If failed, it contains an `ErrorInfo` (or collection) describing the issue (e.g., `NotFound`, `DatabaseError`). The `Try` extensions can be used here to safely wrap operations.
2.  **Application Layer:** An application service receives the `IResult<T>`.
    * It uses `Map` to transform the value if successful (e.g., `DomainModel` to `Dto`).
    * It uses `Bind` or `Then` to chain another operation that also returns an `IResult<U>`.
    * It uses `OnFailure` to log specific errors or `Match` to handle success and failure branches.
    * It then returns its own `IResult<U>` to the presentation layer.
3.  **Presentation Layer (e.g., ASP.NET Core Controller):** The controller receives the `IResult<U>`.
    * It checks `IsSuccess` or `IsFailure`.
    * If successful, it returns an `OkObjectResult` with the `Value`.
    * If failed, it maps the `ErrorInfo` and `IResultStatus` to an appropriate HTTP status code and error response (e.g., `BadRequestObjectResult`, `NotFoundResult`).

---

## 7. Error Handling Strategy

`Zentient.Results` promotes a **structured, explicit, and proactive** error handling strategy:

* **Explicit Error Types:** Instead of relying solely on exception types, `ErrorInfo` provides a common, extensible structure for all errors, now including `Detail` and `Metadata` for richer context.
* **Categorization:** `ErrorCategory` allows for broad classification (e.g., `Validation`, `Authentication`, `ProblemDetails`), enabling generic error handling logic.
* **Specific Codes & Messages:** `Code` and `Message` fields provide granular details for programmatic and human understanding.
* **Contextual Data:** The `Metadata` dictionary allows for attaching arbitrary, relevant context to an error, which is invaluable for debugging and client-side error display.
* **Error Aggregation:** `InnerErrors` supports scenarios where multiple errors occur simultaneously (e.g., multiple validation failures), allowing them to be grouped under a single `ErrorInfo`.
* **Controlled Exception Boundaries:** While favoring explicit results, `ResultException` and `FromException` / `Try` extensions provide mechanisms to bridge between exception-throwing code and the result-based flow where appropriate.

---

## 8. Serialization Strategy

The `ResultJsonConverter` ensures that `Zentient.Results` types are first-class citizens in JSON-based communication:

* **Standard JSON Output:** `Result` and `Result<T>` objects serialize into a predictable JSON structure that now explicitly includes `isSuccess`, `isFailure`, `status`, `messages`, `errors`, `errorMessage`, and `value` (for `Result<T>`). Property naming follows the configured `JsonSerializerOptions.PropertyNamingPolicy`.
* **Robust Round-trip Compatibility:** The custom converter handles both serialization and deserialization. This ensures that `Result` objects can be reliably transmitted and reconstructed, even in cases where parts of the incoming JSON might be incomplete, thanks to built-in deserialization fallbacks.
* **Readability:** The serialized JSON is clear and easy to consume by client applications or other services.

---

## 9. Scalability and Performance Considerations

* **Class-based Results (with immutability):** While `Result` and `Result<T>` are now classes (heap-allocated), their immutable nature reduces the complexity of concurrent access and state management. For typical application scenarios, the benefits in flexibility and robust serialization outweigh the marginal increase in garbage collection pressure.
* **Value Types (`struct` for `ResultStatus`):** `ResultStatus` remains a `readonly struct`, minimizing heap allocations for this frequently used type.
* **Immutability:** Simplifies concurrent access patterns, reducing the need for locks or complex synchronization mechanisms.
* **Lazy Evaluation:** The `_firstError` field uses `Lazy<T>` to ensure that the message extraction logic is only executed when the `ErrorMessage` property is actually accessed, avoiding unnecessary computation.

---

## 10. Extensibility Model

The library provides several extension points:

* **Custom `IResultStatus`:** Implement `IResultStatus` to define domain-specific statuses not covered by `ResultStatuses`. The `ResultStatus.FromHttpStatusCode` method further aids in creating status objects from standard HTTP codes.
* **Custom `ErrorInfo` Metadata:** The `Metadata` dictionary within `ErrorInfo` allows for flexible inclusion of arbitrary custom data without modifying the `ErrorInfo` class itself.
* **Extension Methods:** Developers can write their own extension methods on `IResult` or `IResult<T>` to add domain-specific helper functions or integrate with other libraries.

---

## 11. Conclusion

The architecture of `Zentient.Results` is meticulously designed to provide a robust, predictable, and developer-friendly framework for outcome handling in .NET applications. By embracing immutability, functional composition, and clear separation of concerns, it empowers developers to build more resilient, maintainable, and observable systems, moving towards a more explicit and less error-prone control flow.

**Last Updated:** 2025-06-21 **Version:** 0.4.0
