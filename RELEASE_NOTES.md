# **Zentient.Results 0.4.0**

We're excited to announce the release of **Zentient.Results, version 0.4.0**! This is a significant update that refines the core architecture, enhances immutability, and solidifies JSON serialization, providing developers with even greater control, predictability, and robustness in handling operational outcomes within their .NET applications.

## **What's New in 0.4.0**

This version brings the following major improvements and features:

* **Core Type Refactoring: Result and Result<T> are now sealed classes**:  
  * The core Result and Result<T> types have transitioned from readonly struct to sealed class. This change was made to enhance compatibility with various .NET features, improve internal consistency, and ensure more robust JSON serialization behavior, especially for complex scenarios.  
  * They maintain their immutability post-construction, aligning with functional principles.  
* **Enhanced ErrorInfo Structure and Immutability**:  
  * ErrorInfo is now a **sealed class**, providing a more flexible and robust structure for error details.  
  * The Metadata and InnerErrors collections within ErrorInfo are now explicitly IImmutableDictionary<string, object?> and IImmutableList<ErrorInfo> respectively. The constructor ensures that any provided collections are converted to their immutable counterparts, guaranteeing that ErrorInfo instances are truly immutable after creation.  
  * New internal static factory methods (Unauthorized, Forbidden) have been added to ErrorInfo, providing convenient and standardized ways to create common authentication and authorization related error objects.  
  * Existing ErrorInfo factory methods (Validation, FromException) have been updated to ensure consistency with the new immutability requirements for Metadata.  
* **Refined API Surface and Functional Enhancements**:  
  * The IResult.Error property has been renamed to **IResult.ErrorMessage** for clearer and more intuitive access to the primary error message.  
  * The GetValueOrThrow() method (previously Unwrap()) on Result<T> now offers new overloads, allowing you to provide a custom exception message or a custom exception factory, offering more control over exception propagation at controlled boundaries.  
  * A new **Tap()** extension method has been introduced, enabling the execution of side-effects for successful results without altering the result value or type.  
  * Asynchronous **Bind()** overloads have been added for IResult<T> and IResult, streamlining the chaining of operations that return Task<IResult<T>> or Task<IResult>.  
  * The ResultExtensions class has been logically split into more granular static classes (e.g., ResultConversionExtensions, ResultCreationHelpersExtensions, ResultTransformationExtensions, ResultTryExtensions) to improve discoverability and organization of extension methods.  
* **Extended ResultStatuses and Thread-Safe Caching**:  
  * The ResultStatuses class now includes a more comprehensive set of pre-defined HTTP-aligned statuses, such as TooManyRequests, ImATeapot, and others, providing richer options for status reporting.  
  * The internal caching mechanism for IResultStatus instances within ResultStatuses now uses a ConcurrentDictionary, ensuring thread-safe retrieval and addition of custom statuses.  
* **Robust JSON Serialization with System.Text.Json**:  
  * A dedicated ResultJsonConverter (implementing JsonConverterFactory) now provides robust and explicit serialization and deserialization for Result and Result<T> types, ErrorInfo, and IResultStatus.  
  * This ensures consistent and predictable JSON payloads, making integration with APIs and messaging systems seamless. Properties like isSuccess, isfailure, status, messages, errors, errorMessage, and value (for Result<T>) are explicitly serialized, adhering to JsonSerializerOptions.PropertyNamingPolicy.  
* **Enhanced Code Quality and Reliability**:  
  * The internal project directory structure has been reorganized for improved clarity and maintainability.  
  * Test assertions have been simplified and aligned with the new ErrorInfo structure, reflecting the enhanced immutability.  
  * General refinements and optimizations have been applied across the codebase, including updates to helper classes like Guard.cs.  
  * **Comprehensive Test Coverage**: This release has been rigorously tested and **passes all 166 unit and integration tests**, ensuring high reliability and predictability.

## **Important Notes**

* **Breaking Changes**: This is a minor version upgrade that introduces **breaking changes** due to foundational refactorings.  
  * The most significant change is the transition of Result and Result<T> from struct to **sealed class**. This will require recompilation and may necessitate minor code adjustments if you relied on struct-specific behaviors (e.g., default parameterless constructors for value types).  
  * The IResult.Error property has been renamed to IResult.ErrorMessage. Any direct access to the Error property will need to be updated.  
  * ErrorInfo.Data and ErrorInfo.Extensions properties have been consolidated into ErrorInfo.Metadata. Code accessing these old properties will need to be updated to use Metadata.  
* **Review Your ErrorInfo Usage**: Due to the enhanced immutability and introduction of Metadata, we strongly recommend reviewing how you construct and interact with ErrorInfo instances in your codebase.  
* **Feedback Welcome**: Your feedback is crucial for the continued improvement of Zentient.Results. Please report any issues, suggest improvements, or share your usage experiences by opening an issue on our GitHub repository.

## **Installation**

To update or install this version, you can use NuGet:

```bash
dotnet add package Zentient.Results --version 0.4.0
```

Or using the Package Manager Console:

```powershell
Install-Package Zentient.Results -Version 0.4.0
```

**Created:** 2025-06-21 **Version:** 0.4.0
