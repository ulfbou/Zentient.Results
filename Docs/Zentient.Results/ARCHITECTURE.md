Here's the "Zentient.Results Architecture" document updated to fully comply with the Zentient Framework Documentation Standards, including naming, metadata, and visual formatting.

---

# 📘 Zentient.Results Architecture

📁 Location: `/docs/architecture/zentient-results-architecture.md`
📅 Last Updated: 2025-06-28
📄 Status: ✅ Active Guideline
📦 Module: `Zentient.Results`
🏷️ Version: `v0.4.4`

---

## ➡️ Purpose

`Zentient.Results` is a foundational .NET library for representing the outcome of operations in a standardized, explicit, and immutable manner. It replaces exception-based control flow with result types that encapsulate either a successful value or one or more structured errors. The library is compatible with .NET 6–9 and is designed to support Clean Architecture, CQRS, and Domain-Driven Design (DDD).

---

## 🎯 Architectural Objectives

* **Explicitness:** All operations return results that explicitly indicate success or failure.
* **Structured Errors:** Errors are rich, categorized, and human- and machine-readable.
* **Composability:** Functional chaining of operations is supported for concise, declarative business logic.
* **Immutability:** Result objects are immutable, enabling thread safety and predictable behavior.
* **Extensibility:** Status codes, error categories, and behaviors can be customized.
* **Efficiency:** Lazy evaluation and careful type choices minimize overhead.
* **Minimal Dependencies:** The core remains lightweight and avoids dependency conflicts.
* **Serialization:** Seamless (de)serialization for API and messaging interoperability.

---

## 🧩 Core Components

### Result Types

* **`IResult` / `Result` (non-generic):**
    * Represents the outcome of operations not returning a value.
    * Properties: `IsSuccess`, `IsFailure`, `Errors`, `Messages`, `ErrorMessage`, `Status`.
    * Immutability via `sealed class` and `init` properties.
    * Static factory methods for creating both success and varied failure results.
* **`IResult<T>` / `Result<T>` (generic):**
    * Represents the outcome of operations returning a value of type `T`.
    * Inherits from `IResult`; adds `Value` and functional methods (`Map`, `Bind`, `Match`, etc.).
    * Static factory methods for success (with value) and failure.

### Error Handling

* **`ErrorInfo`:**
    * `Sealed class` encapsulating error details: `Category`, `Code`, `Message`, `Detail`, `Metadata`, `InnerErrors`.
    * Supports error aggregation and arbitrary metadata.
    * Static factory methods for standard error types.
* **`ErrorCategory`:**
    * Enum classifying errors (e.g., `Validation`, `InternalServerError`, `RateLimit`).

### Status Codes

* **`IResultStatus`, `ResultStatus`, `ResultStatuses`:**
    * `IResultStatus` defines the status contract (`Code`, `Description`).
    * `ResultStatus`: `readonly struct`, value equality, HTTP-aligned codes.
    * `ResultStatuses`: thread-safe static registry of common statuses.

### Extensions

* Organized into logical classes for discoverability: Conversion, creation, exception throwing, side effects, status checks, transformation, exception wrapping, and value extraction.

### Serialization

* **`ResultJsonConverter`:**
    * Custom `System.Text.Json.JsonConverterFactory` for serializing/deserializing `Result` and `Result<T>`.
    * Produces predictable, policy-compliant JSON output and robustly handles round-trip scenarios.

### Exception Support

* **`ResultException`:**
    * Exception type for representing failed results explicitly, with structured errors.

---

## ✨ Principles & Patterns

* **Monadic Pattern:**
    * `Map`, `Bind`, `Then` enable operation chaining, short-circuiting on failure.
* **Value Objects:**
    * `ErrorInfo` and `ResultStatus` are immutable value objects.
* **Immutability:**
    * All core types are immutable, ensuring thread safety and predictability.
* **Separation of Concerns:**
    * Outcome, value, status, and error details are clearly separated.
* **Composition over Inheritance:**
    * Error and result aggregation is preferred to inheritance hierarchies.
* **Functional Style:**
    * Fluent APIs encourage declarative, testable code.

---

## 🤝 Integration

* **Application Services:**
    * Methods return `IResult<T>`, communicating outcomes explicitly.
* **ASP.NET Core:**
    * Results map directly to HTTP responses via extensions and the `Zentient.Results.AspNetCore` package.
* **CQRS:**
    * Command/query handlers use results for explicit pipeline error handling.
* **DDD:**
    * Results signal domain operation outcomes and state transitions.
* **Observability:**
    * Structured errors and statuses provide context for logging and monitoring.
* **Resilience:**
    * Error classification supports retry and circuit-breaker strategies.

---

## 🌊 Data Flow Example

* **Infrastructure:**
    * Repository returns `IResult<T>` or `IResult`.
* **Application:**
    * Service transforms or chains results using `Map`, `Bind`, and handles errors with `OnFailure` or `Match`.
* **Presentation:**
    * Controller/action checks result status and maps to HTTP responses.

---

## 🚫 Error Handling

* **Explicit Error Types:**
    * All errors use the structured `ErrorInfo` class.
* **Error Categorization:**
    * Use `ErrorCategory` for broad handling.
* **Granular Codes & Messages:**
    * `Code` and `Message` provide detail for clients and logs.
* **Metadata:**
    * Attach arbitrary debugging or context data.
* **Error Aggregation:**
    * Group multiple errors via `InnerErrors`.
* **Exception Bridge:**
    * `ResultException` and `Try` extensions bridge exception-based and result-based flows.

---

## ↔️ Serialization

* **JSON Output:**
    * Includes all relevant properties; naming follows serializer policy.
* **Round-Trip Safety:**
    * Handles missing or incomplete input gracefully.
* **Readable:**
    * Output is clear for both machines and humans.

---

## 🚀 Performance & Scalability

* **Immutable Classes:**
    * Reduces state management complexity; minor heap overhead is offset by benefits.
* **Value Types for Status:**
    * `ResultStatus` is a lightweight `struct`.
* **Minimal Allocations:**
    * Especially for success cases.
* **Lazy Evaluation:**
    * Used where appropriate for efficiency.

---

## ⚙️ Extensibility

* **Custom Statuses:**
    * Implement or register domain-specific `IResultStatus`.
* **Custom Metadata:**
    * Attach data via `ErrorInfo.Metadata`.
* **Extensions:**
    * Add domain-specific helpers on top of base result types.

---

## 🚦 Summary

`Zentient.Results` delivers a robust, explicit, and composable approach to operational outcome handling in .NET. Its design prioritizes immutability, clarity, and extensibility, supporting resilient and maintainable applications.
