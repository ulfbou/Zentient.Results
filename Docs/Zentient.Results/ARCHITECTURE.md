# Architecture of Zentient.Results

This document provides a comprehensive overview of the architectural design and underlying principles of the `Zentient.Results` library. It aims to clarify the structure, interactions, and rationale behind the components, offering insights for developers, architects, and maintainers.

## 1. Introduction and Purpose

`Zentient.Results` is a foundational .NET library designed to standardize the handling of operational outcomes. It moves beyond traditional exception-based control flow by introducing explicit, immutable result types that encapsulate either a successful value or a structured collection of errors. This architectural approach promotes cleaner code, enhances predictability, simplifies error propagation, and improves the overall observability of application behavior.

The primary goal of `Zentient.Results` is to provide a robust, composable, and extensible framework for communicating success and failure across various layers of a .NET application, aligning with modern software design principles such as Clean Architecture, CQRS, and Domain-Driven Design.

## 2. Architectural Goals

The architecture of `Zentient.Results` is guided by the following key objectives:

* **Predictable Outcomes:** Ensure that the success or failure of an operation is an explicit part of its contract, not an implicit side effect.
* **Structured Error Context:** Provide rich, categorized error information that is easily consumable by both code and humans.
* **Functional Composability:** Enable fluent chaining of operations, allowing for declarative and concise expression of business logic.
* **Immutability:** Guarantee that result objects are immutable after creation, enhancing thread safety and simplifying reasoning about state.
* **Extensibility:** Allow for customization of status codes, error categories, and result behaviors to fit diverse domain requirements.
* **Performance Efficiency:** Utilize value types and lazy evaluation where appropriate to minimize overhead.
* **Minimal Dependencies:** Keep the core library lean to reduce footprint and avoid dependency conflicts.
* **Serialization Friendliness:** Facilitate seamless serialization and deserialization for integration with APIs and messaging systems.

## 3. Core Components

The `Zentient.Results` library is composed of several interconnected components, each serving a distinct purpose:

### 3.1. `IResult` and `Result` (Non-Generic)

* **Role:** Represents the outcome of an operation that does not return a specific value (e.g., a command that modifies state).
* **Key Properties:** `IsSuccess`, `IsFailure`, `Errors` (`IReadOnlyList<ErrorInfo>`), `Messages` (`IReadOnlyList<string>`), `Error` (first error message), `Status` (`IResultStatus`).
* **Design:** `Result` is a `readonly struct`, ensuring immutability and stack allocation benefits. It provides static factory methods for creating success and various failure states (e.g., `Success()`, `Failure()`, `NotFound()`, `Unauthorized()`).

### 3.2. `IResult<T>` and `Result<T>` (Generic)

* **Role:** Represents the outcome of an operation that produces a specific value of type `T`.
* **Key Properties:** Inherits all properties from `IResult`, plus `Value` (`T?`).
* **Design:** `Result<T>` is also a `readonly struct`. It includes additional functional-style methods (`Map`, `Bind`, `OnSuccess`, `OnFailure`, `Match`) to compose operations on the contained `Value`. It provides static factory methods for success with a value, and various failure states.

### 3.3. `ErrorInfo` and `ErrorCategory`

* **Role:** `ErrorInfo` is a structured value type (`readonly struct`) that encapsulates detailed information about a specific error. `ErrorCategory` is an `enum` providing predefined classifications for errors.
* **`ErrorInfo` Properties:** `Category` (`ErrorCategory`), `Code` (`string`), `Message` (`string`), `Data` (`object?`), `InnerErrors` (`IReadOnlyList<ErrorInfo>`).
* **Design:** `ErrorInfo` is designed to be comprehensive and extensible, allowing for rich context to be attached to failures. The `Data` property provides a flexible way to include arbitrary metadata (e.g., validation rule details). `InnerErrors` supports aggregation of multiple related errors.

### 3.4. `IResultStatus`, `ResultStatus`, and `ResultStatuses`

* **Role:** `IResultStatus` defines the contract for an operation's status. `ResultStatus` is the default implementation, and `ResultStatuses` is a static class providing a centralized collection of commonly used `IResultStatus` instances.
* **`IResultStatus` Properties:** `Code` (`int`), `Description` (`string`).
* **Design:** `ResultStatus` is a `readonly struct` and implements `IEquatable<ResultStatus>`, ensuring value equality. `ResultStatuses` pre-populates common HTTP-aligned statuses (e.g., 200 OK, 400 Bad Request) and allows for retrieval of custom statuses, promoting consistency.

### 3.5. `ResultExtensions`

* **Role:** A static class providing a rich set of extension methods for `IResult` and `IResult<T>`.
* **Key Methods:** `IsSuccess()`, `IsFailure()`, `HasErrorCategory()`, `HasErrorCode()`, `OnSuccess()`, `OnFailure()`, `Map()`, `Bind()`, `Unwrap()`, `GetValueOrDefault()`, `ThrowIfFailure()`, `Then()`.
* **Design:** These extensions enhance the fluent API, simplify common result handling patterns, and provide utility for diagnostics and control flow.

### 3.6. `ResultJsonConverter`

* **Role:** A custom `System.Text.Json.JsonConverterFactory` that enables seamless serialization and deserialization of `Result` and `Result<T>` types.
* **Design:** It dynamically creates specific converters (`ResultNonGenericJsonConverter`, `ResultGenericJsonConverter<TValue>`) for the non-generic and generic `Result` types. This ensures that `Result` objects can be correctly represented in JSON payloads, exposing relevant properties like `IsSuccess`, `Errors`, `Messages`, `Status`, and `Value` (for `Result<T>`). It handles the internal `private readonly` fields correctly during serialization/deserialization.

## 4. Architectural Principles and Design Patterns

`Zentient.Results` embodies several key architectural principles and design patterns:

* **Monadic Pattern (Result Monad):** The `Map` and `Bind` methods on `Result<T>` are direct implementations of the `fmap` and `bind` operations found in monadic structures. This enables chaining of operations where the failure of any step short-circuits the entire sequence, propagating the error without explicit `if` checks at each stage.
    * `Map`: Transforms the successful value without changing the result's success/failure state.
    * `Bind`: Allows chaining operations that *also* return a `Result`, effectively flattening nested results.
* **Value Object:** `ErrorInfo` and `ResultStatus` are designed as value objects. They are immutable, their equality is based on their constituent properties, and they encapsulate a concept rather than an identity.
* **Immutability:** All core result types (`Result`, `Result<T>`, `ErrorInfo`, `ResultStatus`) are `readonly struct`s. This design choice guarantees that once a result is created, its state cannot be altered, leading to:
    * **Thread Safety:** No shared mutable state, simplifying concurrent programming.
    * **Predictability:** Easier to reason about code as object state doesn't change unexpectedly.
    * **Referential Transparency:** Functions operating on `Result` objects can be considered "pure" if they only depend on their inputs and produce consistent outputs.
* **Separation of Concerns:**
    * **Outcome vs. Value:** `Result` and `Result<T>` focus solely on the outcome, while the actual data (`T`) or error details (`ErrorInfo`) are encapsulated within.
    * **Status vs. Error:** `IResultStatus` provides a high-level classification (e.g., HTTP status), while `ErrorInfo` provides granular, domain-specific details.
    * **Business Logic vs. Error Handling:** Business logic methods return `Result` objects, cleanly separating the domain operation from the mechanics of error propagation.
* **Composition over Inheritance:** The library favors composing results and their components (e.g., combining `ErrorInfo` instances) rather than relying on complex inheritance hierarchies for different result types.
* **Functional Programming Paradigms:** The fluent API and the monadic-inspired methods encourage a functional style, leading to more declarative, testable, and less error-prone code.

## 5. Integration Points

`Zentient.Results` is designed to integrate seamlessly into various layers and frameworks within a modern .NET application:

* **Application Service Layers:** Services can return `IResult<T>` from their methods, clearly signaling business outcomes (success or specific failures) without throwing exceptions.
* **ASP.NET Core (Controllers & Minimal APIs):** `IResult` and `IResult<T>` can be easily mapped to `IActionResult` types (e.g., `OkObjectResult`, `BadRequestObjectResult`, `NotFoundResult`, `UnprocessableEntityObjectResult`), providing consistent API responses. Custom middleware or extension methods can automate this mapping.
* **CQRS (e.g., MediatR):** Command and query handlers can return `IResult` or `IResult<T>`, allowing for explicit outcome handling within the pipeline, rather than relying on global exception handlers.
* **Logging and Observability:** The structured `ErrorInfo` and `IResultStatus` provide rich context for logging, tracing (e.g., OpenTelemetry span tagging), and monitoring, enabling better diagnostics and incident response.
* **Domain-Driven Design (DDD):** Domain services and aggregates can return `IResult` to indicate the outcome of business operations, reinforcing domain invariants and explicit state transitions.
* **Resilience Patterns:** The categorized `ErrorInfo` and `IResultStatus` can be used to inform resilience strategies (e.g., circuit breakers, retries) by providing clear indications of transient vs. permanent failures.

## 6. Data Flow and Lifecycle

A typical data flow using `Zentient.Results` might look like this:

1.  **Domain/Infrastructure Layer:** A repository or external service call returns an `IResult<T>` (or `IResult`).
    * If successful, it contains the requested data.
    * If failed, it contains an `ErrorInfo` (or collection) describing the issue (e.g., `NotFound`, `DatabaseError`).
2.  **Application Layer:** An application service receives the `IResult<T>`.
    * It uses `Map` to transform the value if successful (e.g., `DomainModel` to `Dto`).
    * It uses `Bind` to chain another operation that also returns an `IResult<U>`.
    * It uses `OnFailure` to log specific errors or `Match` to handle success and failure branches.
    * It then returns its own `IResult<U>` to the presentation layer.
3.  **Presentation Layer (e.g., ASP.NET Core Controller):** The controller receives the `IResult<U>`.
    * It checks `IsSuccess` or `IsFailure`.
    * If successful, it returns an `OkObjectResult` with the `Value`.
    * If failed, it maps the `ErrorInfo` and `IResultStatus` to an appropriate HTTP status code and error response (e.g., `BadRequestObjectResult`, `NotFoundResult`).

## 7. Error Handling Strategy

`Zentient.Results` promotes a **structured, explicit, and proactive** error handling strategy:

* **Explicit Error Types:** Instead of relying solely on exception types, `ErrorInfo` provides a common, extensible structure for all errors.
* **Categorization:** `ErrorCategory` allows for broad classification (e.g., `Validation`, `Authentication`), enabling generic error handling logic.
* **Specific Codes & Messages:** `Code` and `Message` fields provide granular details for programmatic and human understanding.
* **Contextual Data:** The `Data` property allows for attaching arbitrary, relevant context to an error, which is invaluable for debugging and client-side error display.
* **Error Aggregation:** `InnerErrors` supports scenarios where multiple errors occur simultaneously (e.g., multiple validation failures), allowing them to be grouped under a single `ErrorInfo`.

## 8. Serialization Strategy

The `ResultJsonConverter` ensures that `Zentient.Results` types are first-class citizens in JSON-based communication:

* **Standard JSON Output:** `Result` and `Result<T>` objects serialize into a predictable JSON structure that includes `isSuccess`, `isFailure`, `status`, `messages`, `errors`, and `value` (for `Result<T>`).
* **Readability:** The serialized JSON is clear and easy to consume by client applications or other services.
* **Round-trip Compatibility:** The custom converter handles both serialization and deserialization, ensuring that `Result` objects can be reliably transmitted and reconstructed.

## 9. Scalability and Performance Considerations

* **Value Types (`struct`):** The use of `readonly struct` for `Result`, `Result<T>`, `ErrorInfo`, and `ResultStatus` minimizes heap allocations and garbage collection pressure, contributing to better performance, especially in high-throughput scenarios.
* **Immutability:** Simplifies concurrent access patterns, reducing the need for locks or complex synchronization mechanisms.
* **Lazy Evaluation:** The `_firstError` field uses `Lazy<T>` to ensure that the message extraction logic is only executed when the `Error` property is actually accessed, avoiding unnecessary computation.

## 10. Extensibility Model

The library provides several extension points:

* **Custom `IResultStatus`:** Implement `IResultStatus` to define domain-specific statuses not covered by `ResultStatuses`.
* **Custom `ErrorInfo` Fields:** While `ErrorInfo` is a `struct`, its `Data` property allows for flexible inclusion of custom data. For more complex scenarios, one could wrap `ErrorInfo` or use its `Data` property to store serialized custom error objects.
* **Extension Methods:** Developers can write their own extension methods on `IResult` or `IResult<T>` to add domain-specific helper functions or integrate with other libraries.

## 11. Conclusion

The architecture of `Zentient.Results` is meticulously designed to provide a robust, predictable, and developer-friendly framework for outcome handling in .NET applications. By embracing immutability, functional composition, and clear separation of concerns, it empowers developers to build more resilient, maintainable, and observable systems, moving towards a more explicit and less error-prone control flow.
