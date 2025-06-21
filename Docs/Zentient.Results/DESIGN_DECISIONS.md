---
# Design Decisions for Zentient.Results

This document outlines the key design decisions and the rationale behind them for the `Zentient.Results` library. It serves as a reference for understanding the architectural philosophy and the specific choices made during its development.

---

## 1. Purpose of this Document

The `Design_Decisions.md` file aims to provide a comprehensive record of the core architectural and implementation choices made for `Zentient.Results`. It explains the "why" behind the "what," offering insights into the trade-offs considered and the principles that guided the library's evolution. This document is intended for maintainers, contributors, and advanced users who wish to understand the underlying design philosophy.

---

## 2. Core Principles and Goals

The design of `Zentient.Results` is driven by the following core principles:

* **Predictability:** Outcomes of operations should be explicit and consistently communicated.
* **Clarity:** It should be immediately clear whether an operation succeeded or failed, and why.
* **Composability:** Operations should be easily chainable and combinable, promoting a functional programming style.
* **Immutability:** Result objects, once created, should not change, ensuring thread-safety and predictable behavior.
* **Extensibility:** The framework should allow for customization and integration with specific domain requirements.
* **Minimalism:** Keep the core library lean with minimal external dependencies.
* **Observability:** Facilitate the capture and propagation of rich diagnostic information.

---

## 3. Key Design Decisions

### 3.1. Result Type Transition: From `struct` to `class` (`Result`, `Result<T>`)

* **Rationale:**
    * **Evolution of Usage:** While `struct` provided initial performance benefits and enforced immutability, practical usage and upcoming features (like potential inheritance for specialized result types or better handling in certain DI/serialization scenarios) indicated that a `class`-based approach offers greater flexibility without significant drawbacks in typical scenarios.
    * **Consistency:** Aligns better with the behavior of other complex types in the .NET ecosystem, particularly when dealing with polymorphism or nullability.
    * **Serialization Reliability:** Addresses complexities and potential edge cases encountered with `struct` serialization, especially with `System.Text.Json`, leading to a more robust and predictable serialization experience.
* **Implementation:** `Result` and `Result<T>` are now `sealed class` types. They maintain their immutability post-construction through `readonly` fields and `init` accessors for public properties. Comprehensive static factory methods continue to provide a clear and controlled way to create instances in various success and failure states.
* **Trade-off:** Shifting from `struct` to `class` means `Result` instances are now reference types, residing on the heap and subject to garbage collection. For very high-frequency, short-lived `Result` objects, this might introduce a minor increase in GC pressure compared to the `struct` version. However, the benefits in flexibility and serialization outweigh this for most applications.

### 3.2. Enhanced Error Information (`ErrorInfo`)

* **Rationale:**
    * **Richer Context:** A basic error message is often insufficient for comprehensive error handling. Adding properties like `Detail`, `Metadata`, and `InnerErrors` allows for capturing a much deeper and more structured context about why an operation failed. This is invaluable for debugging, logging, and providing granular feedback to clients.
    * **Standardized Error Codes & Categories:** The introduction of `ErrorCategory` enum and `ErrorCodes` constants provides a consistent vocabulary for classifying errors, enabling more intelligent error processing and reporting.
* **Implementation:** `ErrorInfo` is now a `sealed class` to allow for `null` checks and clearer object identity. It includes `Category` (enum), `Code` (string), `Message` (string), `Detail` (string, optional), `Metadata` (immutable dictionary for arbitrary data), and `InnerErrors` (immutable list for hierarchical errors). Static factory methods (`ErrorInfo.FromException`, `ErrorInfo.Validation`, etc.) simplify creation.
* **Trade-off:** The richer structure of `ErrorInfo` means it's a slightly larger object, potentially increasing memory footprint compared to a simple string message. However, the diagnostic and programmatic benefits largely compensate for this.

### 3.3. Separation of Concerns (`IResult`, `IResult<T>`, `IResultStatus`, `ErrorInfo`)

* **Rationale:**
    * **Clear Contracts:** Interfaces (`IResult`, `IResult<T>`, `IResultStatus`) define clear contracts for how results and their statuses behave, promoting testability and mockability.
    * **Rich Error Context:** `ErrorInfo` is a dedicated, structured object for error details (category, code, message, data, detail, extensions, inner errors). This provides far more context than a simple boolean flag or string message, crucial for debugging, logging, and client-side error handling.
    * **Extensibility:** Users can implement custom `IResultStatus` types or leverage the flexible `ErrorInfo` structure for specific domain requirements.
* **Implementation:** `Result` and `Result<T>` internally manage arrays of `ErrorInfo` and `string` for errors and messages, respectively, exposed as `IReadOnlyList<T>` to maintain immutability from the outside.

### 3.4. Explicit Result Passing vs. Exceptions

* **Rationale:**
    * **Control Flow Clarity:** Exceptions are designed for truly *exceptional* and *unrecoverable* events (e.g., out-of-memory, network cable unplugged). Using them for expected business outcomes (e.g., "user not found," "invalid input") blurs the line between control flow and error conditions, leading to less readable code and potential performance overhead.
    * **Method Signature Transparency:** `IResult<T>` in a method signature explicitly communicates that the method might fail and provides a structured way to handle that failure, unlike `void` or `T` methods that might implicitly throw exceptions.
    * **Composability:** `Result` types enable functional composition (e.g., `Map`, `Bind`) that is difficult and cumbersome to achieve with `try-catch` blocks.
* **Note:** `Zentient.Results` does not replace exceptions entirely. It provides `Result.FromException` and `Result<T>.FromException` to encapsulate exceptions as `ErrorInfo`, automatically passing the `Exception` object as `data` for richer context. This allows exceptions to be propagated within the result flow if necessary, without forcing `try-catch` blocks everywhere. New `Try` extension methods simplify wrapping code that might throw exceptions into a `Result`.

### 3.5. Extensible Status System (`IResultStatus` and `ResultStatus`)

* **Rationale:**
    * **Flexibility:** While `ResultStatuses` provides common HTTP-aligned statuses, `IResultStatus` allows users to define custom statuses relevant to their domain (e.g., `OrderPartiallyFulfilled`, `InsufficientStock`).
    * **Decoupling:** Separating the status from the core `Result` type ensures that the `Result` itself remains generic, while the specific meaning of an outcome is encapsulated in the status object.
* **Implementation:** `ResultStatus` is a `readonly struct` and implements `IEquatable<ResultStatus>`, ensuring value equality and immutability. `ResultStatuses` is a static class providing pre-defined, commonly used `IResultStatus` instances, now including a much broader range of HTTP status codes (1xx, 3xx, and expanded 4xx/5xx). A new static `ResultStatus.FromHttpStatusCode(int statusCode)` method simplifies creating or retrieving `IResultStatus` instances directly from HTTP status codes, and `ResultStatuses` now uses a `ConcurrentDictionary` for thread-safe caching of custom statuses.

### 3.6. `System.Text.Json` Compatibility (`ResultJsonConverter`)

* **Rationale:**
    * **API Integration:** Modern .NET applications heavily rely on JSON for communication (e.g., REST APIs, message queues). Seamless serialization and deserialization of `Result` objects are crucial for consistent data contracts.
    * **Control over Serialization:** A custom `JsonConverterFactory` allows precise control over how `Result` and `Result<T>` are serialized and deserialized, ensuring that only relevant public properties are exposed and that the internal state is handled correctly. The transition from `struct` to `class` also significantly improves the reliability and ease of implementing custom serialization.
* **Implementation:** The `ResultJsonConverter` dynamically creates generic and non-generic converters (`ResultGenericJsonConverter<TValue>`, `ResultNonGenericJsonConverter`) to handle both `Result` and `Result<T>` types. It explicitly serializes all critical public properties (`IsSuccess`, `IsFailure`, `Status`, `Messages`, `Errors`, `ErrorMessage`, and `Value` for `Result<T>`). For deserialization, it includes robust logic, defaulting to `ResultStatuses.Error` and injecting a descriptive `ErrorInfo` if the status property is missing or cannot be deserialized, making the process more fault-tolerant. Property names are now converted according to `JsonSerializerOptions.PropertyNamingPolicy`.

### 3.7. Minimal Dependencies

* **Rationale:**
    * **Reduced Footprint:** Keeping external dependencies to a minimum reduces the overall size of the NuGet package and the transitive dependency graph for consuming projects.
    * **Avoid Dependency Conflicts:** Fewer dependencies mean less chance of version conflicts with other libraries in a large solution.
    * **Stability:** Relying only on core .NET features (like `System.Text.Json` which is built-in) enhances the library's stability and reduces exposure to breaking changes in third-party libraries.

### 3.8. Lazy Error Message Evaluation (`_firstError` in `Result<T>` and `Result`)

* **Rationale:**
    * **Performance Optimization:** The `ErrorMessage` property (which provides the first error message) is backed by a `Lazy<string?>`. This means the logic to extract the first error message is only executed if the `ErrorMessage` property is actually accessed, avoiding unnecessary computation for successful results or when only the `Errors` collection is needed.

### 3.9. Implicit Operators

* **Rationale:**
    * **Convenience & Ergonomics:** Implicit operators (e.g., `implicit operator Result<T>(T value)`) provide a more concise and natural syntax for converting a raw value into a successful `Result<T>`. This improves developer experience by reducing boilerplate.
* **Consideration:** While convenient, implicit operators can sometimes lead to ambiguity or unexpected conversions if not used carefully. In `Zentient.Results`, they are designed to be intuitive and cover common, clear conversion scenarios.

### 3.10. `ResultStatuses` Static Class

* **Rationale:**
    * **Centralized Definitions:** Provides a single, well-known location for standard result statuses, improving consistency across applications.
    * **Type Safety:** Using static properties like `ResultStatuses.Success` is more type-safe and less prone to typos than stringly-typed status codes.
    * **Extensibility:** Includes a `GetStatus` method that allows for retrieving or creating custom statuses on the fly, providing flexibility while maintaining a thread-safe registry.

### 3.11. `ResultExtensions` Static Class (Now grouped into several extension classes)

* **Rationale:**
    * **Utility & Convenience:** Provides a set of extension methods that simplify common operations on `IResult` and `IResult<T>` instances.
    * **Readability & Discoverability:** The `ResultExtensions` class has been refactored and split into more focused static classes (e.g., `ResultConversionExtensions`, `ResultCreationHelpersExtensions`, `ResultExceptionThrowingExtensions`, `ResultSideEffectExtensions`, `ResultStatusCheckExtensions`, `ResultTransformationExtensions`, `ResultTryExtensions`, `ResultValueExtractionExtensions`). This improves logical grouping, makes methods easier to find, and clarifies their purpose.
    * **Fluent API:** These extensions continue to make the code using `Zentient.Results` more expressive and readable, aligning with the fluent API design.

---

## 4. Trade-offs

While `Zentient.Results` offers significant benefits, some trade-offs were considered:

* **Learning Curve:** Developers new to functional programming concepts or result monads might experience a slight learning curve initially.
* **Increased Verbosity (in some cases):** For very simple operations, explicitly returning a `Result` might seem more verbose than just throwing an exception. However, this verbosity pays off in clarity and maintainability as complexity grows.
* **Heap Allocation (for Result classes):** The transition from `struct` to `class` for `Result` and `Result<T>` means instances are now heap-allocated, potentially increasing garbage collection overhead for very high-volume scenarios. This is generally mitigated by the immutable nature and typical usage patterns.
* **Boxing for `object? Data`:** The `Metadata` dictionary in `ErrorInfo` stores `object?` values. While flexible, using `object` can lead to boxing/unboxing for value types, incurring minor performance overhead. This was chosen for maximum flexibility in attaching arbitrary context.

---

## 5. Future Considerations and Evolution

The design of `Zentient.Results` allows for future enhancements, including:

* **Asynchronous Bind/Map:** Further asynchronous extensions for `Task<IResult<T>>` to streamline async workflows. (Some already exist in `ResultTransformationExtensions`).
* **Integration with Validation Libraries:** Potential for direct integration points with popular .NET validation libraries (e.g., FluentValidation) to easily convert validation results into `ErrorInfo` collections.
* **More Specialized Error Categories:** Expanding the `ErrorCategory` enum or providing guidance for domain-specific error categorization.
* **Performance Optimizations:** Continuous profiling and optimization, especially around allocations and common paths.

This document will be updated as the library evolves and new design decisions are made.

**Last Updated:** 2025-06-21 **Version:** 0.4.0
