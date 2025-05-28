# Zentient.Results Release Notes

## Version 0.1.0 - May 28, 2025

We are thrilled to announce the official release of **Zentient.Results**, a foundational library designed to streamline the handling of operation outcomes in .NET applications. This initial stable version provides a robust and intuitive way to represent success and failure states, promoting clearer code, improved error handling, and more predictable application behavior.

Zentient.Results empowers developers to move beyond traditional exception-based error handling for expected outcomes, offering a more explicit and type-safe approach. By encapsulating results with clear indicators of success or failure, along with detailed error information, developers can build more resilient and maintainable systems.

### ✨ Key Features

This release introduces the following core components and capabilities:

* **Explicit Result Representation:**
    * **`IResult` and `Result<T>` Structs**: Core immutable structs that clearly distinguish between successful operations (which may optionally carry a value) and failed ones. These structs provide properties like `IsSuccess`, `IsFailure`, and access to detailed error information.
    * **`IResultStatus` Interface and `DefaultResultStatus` Struct**: A standardized way to convey the status of an operation, typically aligning with HTTP status codes. `DefaultResultStatus` offers a concrete implementation with properties for `Code` and `Description`.

* **Comprehensive Error Handling:**
    * **`ErrorCategory` Enum**: A predefined set of categories to classify the nature of an error (e.g., `Validation`, `Authentication`, `NotFound`, `Database`, `Exception`, `Network`). This allows for more granular error reporting and handling.
    * **`ErrorInfo` Struct**: A rich, immutable structure for detailing errors, including its `Category`, a specific `Code`, a human-readable `Message`, optional `Data` for context, and support for `InnerErrors` to represent aggregated or nested failures.

* **Intuitive Factory Methods:**
    * **Success Scenarios**: Dedicated static methods like `Success()`, `Created()`, and `NoContent()` for easily creating successful result instances.
    * **Failure Scenarios**: A wide array of static methods for common error conditions, including `Failure()` (for single or multiple `ErrorInfo` instances), `Validation()`, `Unauthorized()`, `Forbidden()`, `NotFound()`, `Conflict()`, `InternalError()`, and `FromException()`. This simplifies the creation of error results with appropriate status and error details.

* **Functional Programming Constructs (for `Result<T>`):**
    * **`Map<U>()`**: Transforms the successful value into a new type, propagating errors if the original result was a failure.
    * **`Bind<U>()`**: Chains operations where the next step also returns a `Result<U>`, effectively flattening nested results.
    * **`Tap()` / `OnSuccess()`**: Executes an action only if the result is successful, allowing for side effects without altering the result.
    * **`OnFailure()`**: Executes an action only if the result is a failure, useful for logging or specific error handling.
    * **`Match<U>()`**: Enables elegant pattern matching over success and failure states, providing a clean way to handle both outcomes.

* **Value Access Strategies (for `Result<T>`):**
    * **`GetValueOrThrow()`**: Retrieves the value if successful, or throws an `InvalidOperationException` (or a custom exception via factory) if a failure.
    * **`GetValueOrDefault()`**: Provides a fallback value if the result is a failure or the value is null.

* **Seamless JSON Serialization:**
    * **`ResultJsonConverter`**: A custom `JsonConverterFactory` ensures that `Result` and `Result<T>` instances can be correctly serialized to and deserialized from JSON, preserving all relevant information including status, messages, errors, and the encapsulated value.

* **Predefined Result Statuses:**
    * **`ResultStatuses` Static Class**: Offers a convenient collection of pre-defined `IResultStatus` instances covering common HTTP status codes (e.g., 200 OK, 201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error).

* **Extension Methods:**
    * **`ResultStatusExtensions.ToHttpStatusCode()`**: Provides a straightforward way to get the integer HTTP status code from an `IResultStatus` instance.

### ⬆️ Upgrade Notes

As this is the initial release, there are no specific upgrade considerations from previous versions.

### 🛠️ Usage

To get started with Zentient.Results, simply install the NuGet package. Detailed examples and usage guidelines will be provided in the accompanying documentation.

### 📚 Documentation

Comprehensive documentation for Zentient.Results will be available shortly, covering installation, API reference, and practical usage patterns.

### 🐞 Known Issues

There are no known issues in this initial release.

### 🤝 Contributing

We welcome contributions from the community! If you have suggestions, bug reports, or would like to contribute code, please visit our GitHub repository (link to be provided).

---
*Zentient.Results Team*
