# Zentient.Results

[](https://www.nuget.org/packages/Zentient.Results/)
[](https://github.com/ulfbou/Zentient.Results/blob/main/LICENSE)

Zentient.Results is a lightweight, opinionated .NET library that provides a robust and consistent approach to handling operation outcomes. It introduces immutable result types to encapsulate success values or structured error information, promoting clean architecture principles, enhancing code readability, and streamlining error propagation across application layers. This framework is designed to align with modern functional programming paradigms and improve the predictability of your application's behavior.

## Key Features

  * **Immutable Result Types:** `Result` (for void operations) and `Result<T>` (for operations returning a value) ensure that outcomes are fixed once created, promoting predictable data flow.
  * **Structured Error Information:** Failures are represented by `ErrorInfo` objects, providing categorized codes, human-readable messages, and optional contextual data or inner errors.
  * **Fluent API for Composition:** Enables chaining and transforming results using methods like `Map`, `Bind`, `OnSuccess`, and `OnFailure`, facilitating clean, composable business logic.
  * **Clear Status Separation:** Distinguishes between the operation's outcome (`IsSuccess`/`IsFailure`) and its detailed status (`IResultStatus`), which can align with HTTP status codes or custom domain-specific states.
  * **Extensible Status System:** The `IResultStatus` interface allows for the definition of custom result statuses beyond standard HTTP responses, providing flexibility for domain-specific outcomes.
  * **Built-in HTTP Status Alignment:** Predefined `ResultStatus` constants (e.g., `ResultStatuses.Success`, `ResultStatuses.BadRequest`, `ResultStatuses.NotFound`) simplify mapping results to standard HTTP responses.
  * **Lightweight and Minimal Dependencies:** Designed with minimal external dependencies to ensure a small footprint and easy integration into any .NET project.

## Installation

Install Zentient.Results via NuGet Package Manager:

```bash
dotnet add package Zentient.Results
```

Zentient.Results targets `.NET 6+`, `.NET 7+`, `.NET 8+`, and `.NET 9`.

## Getting Started

Here's a quick example demonstrating basic usage:

```csharp
using Zentient.Results;
using System;
using System.Collections.Generic;

public class UserService
{
    // Example: Operation that can succeed or fail
    public IResult<User> GetUserById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Result<User>.Failure(
                default,
                new ErrorInfo(ErrorCategory.Validation, "InvalidId", "User ID cannot be empty."),
                ResultStatuses.BadRequest);
        }

        // Simulate fetching a user
        if (id == new Guid("A0000000-0000-0000-0000-000000000001"))
        {
            return Result<User>.Success(new User { Id = id, Name = "Alice" });
        }
        
        return Result<User>.Failure(
            default,
            new ErrorInfo(ErrorCategory.NotFound, "UserNotFound", $"User with ID {id} was not found."),
            ResultStatuses.NotFound);
    }

    // Example: Chaining operations
    public IResult<string> GetUserName(Guid userId)
    {
        return GetUserById(userId)
            .Map(user => user.Name) // Extract user name if successful
            .OnFailure(errors => Console.WriteLine($"Failed to get user name: {errors[0].Message}")); // Log error
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var userService = new UserService();

        // Successful call
        var successResult = userService.GetUserById(new Guid("A0000000-0000-0000-0000-000000000001"));
        if (successResult.IsSuccess)
        {
            Console.WriteLine($"User found: {successResult.Value.Name}");
        }

        // Failed call - User not found
        var notFoundResult = userService.GetUserById(Guid.NewGuid());
        if (notFoundResult.IsFailure)
        {
            Console.WriteLine($"Error: {notFoundResult.Error}");
            Console.WriteLine($"Status Code: {notFoundResult.Status.Code}, Description: {notFoundResult.Status.Description}");
        }

        // Chained operation
        var chainedSuccess = userService.GetUserName(new Guid("A0000000-0000-0000-0000-000000000001"));
        if (chainedSuccess.IsSuccess)
        {
            Console.WriteLine($"Chained user name: {chainedSuccess.Value}");
        }

        var chainedFailure = userService.GetUserName(Guid.Empty); // Will trigger validation error
        // OnFailure action in GetUserName will be executed here
    }
}
```

## Usage Scenarios

Zentient.Results is particularly effective in scenarios requiring robust error handling and clear communication of operational outcomes:

  * **Application Service Layers:** Return `IResult` or `IResult<T>` from business logic methods to clearly indicate success or specific failure conditions, separating concerns from presentation.
  * **Controller Result Handling:** Directly map `IResult` instances from service calls to appropriate ASP.NET Core `IActionResult` types (e.g., `Ok`, `BadRequest`, `NotFound`, `UnprocessableEntity`).
  * **Background Jobs or Middleware:** Standardize the reporting of execution outcomes, including detailed error diagnostics for logging and monitoring.
  * **Command and Query Handlers (CQRS):** Provide explicit results from command execution and query processing, facilitating predictable state changes and data retrieval.
  * **Integration with CI/CD Pipelines:** Use the structured error information to drive automated validation and error reporting in build and deployment processes.

## Architecture & Design Philosophy

Zentient.Results is built on several core design principles:

  * **Composition over Inheritance:** The framework favors composing `Result` objects with specific status and error information rather than relying on complex inheritance hierarchies for different outcome types.
  * **Encapsulation of Status Logic:** `IResultStatus` and `ErrorInfo` provide a dedicated, extensible mechanism for describing the outcome, keeping the core `Result` types focused on flow control.
  * **Separation from Exception-Based Flow:** While compatible with exceptions (e.g., `Result.FromException`), the primary goal is to shift from using exceptions for expected control flow to explicit result passing. Exceptions are reserved for truly exceptional and unrecoverable scenarios.
  * **Immutability and Predictability:** All core result types are `readonly struct`s, ensuring that once an outcome is determined, it cannot be inadvertently modified, leading to more predictable code behavior and easier debugging.

## Advanced Topics

  * **Custom `IResultStatus` Implementations:** Extend `ResultStatus` or implement `IResultStatus` to define domain-specific success or failure states that are not directly mapped to HTTP.
  * **ASP.NET Core Integration:** Leverage extension methods or middleware to automatically convert `IResult` types returned from controllers into appropriate `IActionResult` responses.
  * **Serialization Behavior:** `Result` and `Result<T>` are designed to be compatible with `System.Text.Json` for seamless serialization, typically exposing `IsSuccess`, `Errors`, `Messages`, and `Status` properties.

For more in-depth examples and advanced usage patterns, please refer to the [Zentient.Results Wiki](https://github.com/ulfbou/Zentient.Results/wiki).

## Contributing

We welcome contributions to Zentient.Results\! Please refer to our [CONTRIBUTING.md](https://github.com/ulfbou/Zentient.Results/blob/main/CONTRIBUTING.md) for guidelines on how to submit issues, propose features, or contribute code. We adhere to standard .NET coding conventions.

## License

Zentient.Results is licensed under the [MIT License](https://github.com/ulfbou/Zentient.Results/blob/main/LICENSE).

## Support & Contact

For any issues, questions, or feature requests, please use the [GitHub Issues](https://github.com/ulfbou/Zentient.Results/issues) page.
