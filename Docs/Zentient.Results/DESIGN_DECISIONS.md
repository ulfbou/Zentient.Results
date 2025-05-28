# Design Decisions for Zentient.Results

This document outlines the key design decisions and the rationale behind them for the `Zentient.Results` library. It serves as a reference for understanding the architectural philosophy and the specific choices made during its development.

## 1. Purpose of this Document

The `Design_Decisions.md` file aims to provide a comprehensive record of the core architectural and implementation choices made for `Zentient.Results`. It explains the "why" behind the "what," offering insights into the trade-offs considered and the principles that guided the library's evolution. This document is intended for maintainers, contributors, and advanced users who wish to understand the underlying design philosophy.

## 2. Core Principles and Goals

The design of `Zentient.Results` is driven by the following core principles:

* **Predictability:** Outcomes of operations should be explicit and consistently communicated.
* **Clarity:** It should be immediately clear whether an operation succeeded or failed, and why.
* **Composability:** Operations should be easily chainable and combinable, promoting a functional programming style.
* **Immutability:** Result objects, once created, should not change, ensuring thread-safety and predictable behavior.
* **Extensibility:** The framework should allow for customization and integration with specific domain requirements.
* **Minimalism:** Keep the core library lean with minimal external dependencies.
* **Observability:** Facilitate the capture and propagation of rich diagnostic information.

## 3. Key Design Decisions

### 3.1. Immutable Value Types (`readonly struct Result`, `readonly struct Result<T>`)

* **Rationale:**
    * **Predictability & Thread Safety:** Being `readonly struct` ensures that instances are immutable after creation. This eliminates side effects and makes `Result` objects inherently thread-safe, simplifying concurrent programming.
    * **Performance:** Value types are allocated on the stack (or inline in objects), reducing garbage collection pressure compared to reference types, especially for frequently created short-lived objects.
    * **Semantic Clarity:** A result represents a snapshot of an operation's outcome, which aligns well with the immutability of value types.
* **Trade-off:** While generally beneficial, passing large `struct` instances can sometimes incur a performance penalty due to copying. However, for `Result` types, the internal state (`_errors`, `_messages`, `Status`, `Value`) typically consists of references or small value types, mitigating this concern.

### 3.2. Separation of Concerns (`IResult`, `IResult<T>`, `IResultStatus`, `ErrorInfo`)

* **Rationale:**
    * **Clear Contracts:** Interfaces (`IResult`, `IResult<T>`, `IResultStatus`) define clear contracts for how results and their statuses behave, promoting testability and mockability.
    * **Rich Error Context:** `ErrorInfo` is a dedicated, structured object for error details (category, code, message, data, inner errors). This provides far more context than a simple boolean flag or string message, crucial for debugging, logging, and client-side error handling.
    * **Extensibility:** Users can implement custom `IResultStatus` types or extend `ErrorInfo` if needed, although the provided implementations cover most common scenarios.
* **Implementation:** `Result` and `Result<T>` internally manage arrays of `ErrorInfo` and `string` for errors and messages, respectively, exposed as `IReadOnlyList<T>` to maintain immutability from the outside.

### 3.3. Explicit Result Passing vs. Exceptions

* **Rationale:**
    * **Control Flow Clarity:** Exceptions are designed for truly *exceptional* and *unrecoverable* events (e.g., out-of-memory, network cable unplugged). Using them for expected business outcomes (e.g., "user not found," "invalid input") blurs the line between control flow and error conditions, leading to less readable code and potential performance overhead.
    * **Method Signature Transparency:** `IResult<T>` in a method signature explicitly communicates that the method might fail and provides a structured way to handle that failure, unlike `void` or `T` methods that might implicitly throw exceptions.
    * **Composability:** `Result` types enable functional composition (e.g., `Map`, `Bind`) that is difficult and cumbersome to achieve with `try-catch` blocks.
* **Note:** `Zentient.Results` does not replace exceptions entirely. It provides `Result.FromException` and `Result<T>.FromException` to encapsulate exceptions as `ErrorInfo` when they occur, allowing them to be propagated within the result flow if necessary, without forcing `try-catch` blocks everywhere.

### 3.4. Functional Composition (`Map`, `Bind`, `OnSuccess`, `OnFailure`, `Match`, `Then`, `Tap`)

* **Rationale:**
    * **Readability & Conciseness:** These methods enable a fluent, declarative style of programming, reducing nested `if` statements and `try-catch` blocks. They allow developers to express sequences of operations and their error handling in a more readable and maintainable way.
    * **Error Short-Circuiting:** `Map` and `Bind` automatically propagate failures, meaning subsequent operations in a chain are only executed if the preceding operation was successful. This simplifies error handling logic significantly.
    * **Mimicking Monadic Patterns:** These methods are inspired by monadic patterns (specifically, the `Either` or `Result` monad in functional programming), providing a powerful abstraction for handling computations that might fail.

### 3.5. Extensible Status System (`IResultStatus` and `ResultStatus`)

* **Rationale:**
    * **Flexibility:** While `ResultStatuses` provides common HTTP-aligned statuses, `IResultStatus` allows users to define custom statuses relevant to their domain (e.g., `OrderPartiallyFulfilled`, `InsufficientStock`).
    * **Decoupling:** Separating the status from the core `Result` type ensures that the `Result` itself remains generic, while the specific meaning of an outcome is encapsulated in the status object.
* **Implementation:** `ResultStatus` is a `readonly struct` implementing `IResultStatus` and `IEquatable<ResultStatus>`, ensuring value equality and immutability. `ResultStatuses` is a static class providing pre-defined, commonly used `IResultStatus` instances.

### 3.6. `System.Text.Json` Compatibility (`ResultJsonConverter`)

* **Rationale:**
    * **API Integration:** Modern .NET applications heavily rely on JSON for communication (e.g., REST APIs, message queues). Seamless serialization and deserialization of `Result` objects are crucial for consistent data contracts.
    * **Control over Serialization:** A custom `JsonConverterFactory` allows precise control over how `Result` and `Result<T>` are serialized and deserialized, ensuring that only relevant public properties are exposed and that the internal state (`_errors`, `_messages`) is handled correctly.
* **Implementation:** The `ResultJsonConverter` dynamically creates generic and non-generic converters (`ResultGenericJsonConverter<TValue>`, `ResultNonGenericJsonConverter`) to handle both `Result` and `Result<T>` types. It explicitly serializes `IsSuccess`, `IsFailure`, `Status`, `Messages`, `Errors`, and `Value` (for `Result<T>`).

### 3.7. Minimal Dependencies

* **Rationale:**
    * **Reduced Footprint:** Keeping external dependencies to a minimum reduces the overall size of the NuGet package and the transitive dependency graph for consuming projects.
    * **Avoid Dependency Conflicts:** Fewer dependencies mean less chance of version conflicts with other libraries in a large solution.
    * **Stability:** Relying only on core .NET features (like `System.Text.Json` which is built-in) enhances the library's stability and reduces exposure to breaking changes in third-party libraries.

### 3.8. Lazy Error Message Evaluation (`_firstError` in `Result<T>`)

* **Rationale:**
    * **Performance Optimization:** The `Error` property (which provides the first error message) is backed by a `Lazy<string?>`. This means the logic to extract the first error message is only executed if the `Error` property is actually accessed, avoiding unnecessary computation for successful results or when only the `Errors` collection is needed.

### 3.9. Implicit Operators

* **Rationale:**
    * **Convenience & Ergonomics:** Implicit operators (e.g., `implicit operator Result<T>(T value)`, `implicit operator Result(ErrorInfo error)`) provide a more concise and natural syntax for converting a raw value into a successful `Result<T>` or an `ErrorInfo` into a failed `Result`. This improves developer experience by reducing boilerplate.
    * **Readability:** Allows for more fluid assignments and returns without explicit `Result<T>.Success(value)` or `Result.Failure(error)` calls in simple cases.
* **Consideration:** While convenient, implicit operators can sometimes lead to ambiguity or unexpected conversions if not used carefully. In `Zentient.Results`, they are designed to be intuitive and cover common, clear conversion scenarios.

### 3.10. `ResultStatuses` Static Class

* **Rationale:**
    * **Centralized Definitions:** Provides a single, well-known location for standard result statuses, improving consistency across applications.
    * **Type Safety:** Using static properties like `ResultStatuses.Success` is more type-safe and less prone to typos than stringly-typed status codes.
    * **Extensibility:** Includes a `GetStatus` method that allows for retrieving or creating custom statuses on the fly, providing flexibility while maintaining a registry.

### 3.11. `ResultExtensions` Static Class

* **Rationale:**
    * **Utility & Convenience:** Provides a set of extension methods that simplify common operations on `IResult` and `IResult<T>` instances, such as `IsSuccess`, `IsFailure`, `HasErrorCategory`, `Unwrap`, and various `OnSuccess`/`OnFailure` overloads.
    * **Readability:** These extensions make the code using `Zentient.Results` more expressive and readable, aligning with the fluent API design.

## 4. Trade-offs

While `Zentient.Results` offers significant benefits, some trade-offs were considered:

* **Learning Curve:** Developers new to functional programming concepts or result monads might experience a slight learning curve initially.
* **Increased Verbosity (in some cases):** For very simple operations, explicitly returning a `Result` might seem more verbose than just throwing an exception. However, this verbosity pays off in clarity and maintainability as complexity grows.
* **Boxing for `object? Data`:** The `Data` property in `ErrorInfo` is `object?`. While flexible, using `object` can lead to boxing/unboxing for value types, incurring minor performance overhead. This was chosen for maximum flexibility in attaching arbitrary context.

## 5. Future Considerations and Evolution

The design of `Zentient.Results` allows for future enhancements, including:

* **Asynchronous Bind/Map:** Further asynchronous extensions for `Task<IResult<T>>` to streamline async workflows. (Some already exist in `ResultExtensions`).
* **Integration with Validation Libraries:** Potential for direct integration points with popular .NET validation libraries (e.g., FluentValidation) to easily convert validation results into `ErrorInfo` collections.
* **More Specialized Error Categories:** Expanding the `ErrorCategory` enum or providing guidance for domain-specific error categorization.
* **Performance Optimizations:** Continuous profiling and optimization, especially around allocations and common paths.

This document will be updated as the library evolves and new design decisions are made.
