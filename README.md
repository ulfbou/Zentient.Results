# ✨ Zentient.Results

📁 Location: `/README.md`  
📅 Last Updated: 2025-06-28  
📄 Status: ✅ Active Guideline  
📦 Module: `Zentient.Results`  
🏷️ Version: `v0.4.4`  

---

[![NuGet](https://img.shields.io/nuget/v/Zentient.Results.svg?style=flat-square&label=NuGet)](https://www.nuget.org/packages/Zentient.Results/)  
[![License](https://img.shields.io/github/license/ulfbou/Zentient.Results?style=flat-square)](https://github.com/ulfbou/Zentient.Results/blob/main/LICENSE)  
[![CI & Release](https://github.com/ulfbou/Zentient.Results/actions/workflows/ci.yml/badge.svg?branch=main&style=flat-square)](https://github.com/ulfbou/Zentient.Results/actions/workflows/ci.yml)  

---

## ➡️ Purpose

`Zentient.Results` is a lightweight, opinionated .NET library for robust, explicit, and predictable outcome handling. It introduces **immutable result types** to encapsulate either a successful value or structured error information, supporting clean architecture, readable code, and composable error propagation across application layers.

---

## 📝 Context

Modern .NET applications require clear separation of business logic and error management. `Zentient.Results` replaces exception-driven flows with explicit, functional result types, supporting patterns like CQRS and Domain-Driven Design.

---

## 💡 Key Features

- **Immutable Results:** `Result` (for void operations) and `Result<T>` (for value-returning operations) are sealed and immutable.
- **Comprehensive ErrorInfo:** Rich, categorized error objects with codes, messages, technical details, and metadata.
- **Fluent Composition:** Methods like `Map`, `Bind` (including async), `Then`, `OnSuccess`, and `OnFailure` for expressive business logic.
- **Result Status System:** Distinct outcome (`IsSuccess` / `IsFailure`) and extensible status (`IResultStatus`), with common HTTP-aligned presets.
- **Controlled Exception Bridge:** `ResultException` and conversion helpers for seamless transition between exception and result flows.
- **Serialization Ready:** Full support for JSON serialization via `System.Text.Json` and custom converters.
- **Minimal Dependencies:** Minimal footprint for easy integration into any .NET 6+ project.

---

## ⬇️ Installation

Install via NuGet:

```bash
dotnet add package Zentient.Results
```

Supports `.NET 6+`, `.NET 7+`, `.NET 8+`, `.NET 9`.

---

## 🚀 Getting Started

```csharp
using Zentient.Results;

public class UserService
{
    public IResult<User> GetUserById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Result<User>.BadRequest(
                ErrorInfo.Validation("InvalidId", "User ID cannot be empty.", detail: "A GUID of all zeros is not permitted.")
            );
        }

        if (id == new Guid("A0000000-0000-0000-0000-000000000001"))
        {
            return Result<User>.Success(new User { Id = id, Name = "Alice" }, "User fetched successfully.");
        }

        return Result<User>.NotFound(
            ErrorInfo.NotFound("UserNotFound", $"User with ID {id} was not found.", detail: "The user was not found in the database.")
        );
    }

    public IResult<string> GetUserName(Guid userId)
    {
        return GetUserById(userId)
            .Map(user => user.Name)
            .OnFailure(errors => Console.WriteLine($"Failed to get user name: {errors.FirstOrDefault()?.Message ?? "Unknown error"}"));
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

        var successResult = userService.GetUserById(new Guid("A0000000-0000-0000-0000-000000000001"));
        if (successResult.IsSuccess)
        {
            Console.WriteLine($"User found: {successResult.Value.Name}");
            Console.WriteLine($"Success Message: {successResult.Messages.FirstOrDefault()}");
        }

        Console.WriteLine("---");

        var notFoundResult = userService.GetUserById(Guid.NewGuid());
        if (notFoundResult.IsFailure)
        {
            Console.WriteLine($"Error: {notFoundResult.ErrorMessage}");
            Console.WriteLine($"Status Code: {notFoundResult.Status.Code}, Description: {notFoundResult.Status.Description}");
            Console.WriteLine($"Error Detail: {notFoundResult.Errors.FirstOrDefault()?.Detail}");
        }

        Console.WriteLine("---");

        var chainedSuccess = userService.GetUserName(new Guid("A0000000-0000-0000-0000-000000000001"));
        if (chainedSuccess.IsSuccess)
        {
            Console.WriteLine($"Chained user name: {chainedSuccess.Value}");
        }

        Console.WriteLine("---");

        var chainedFailure = userService.GetUserName(Guid.Empty);
        if (chainedFailure.IsFailure)
        {
            Console.WriteLine($"Chained operation failed with status: {chainedFailure.Status.Description}");
        }
    }
}
```

---

## ✔️ Usage Scenarios

- **Service Layers:** Return `IResult`/`IResult<T>` from business logic for explicit outcome signaling.
- **Controllers:** Map results to ASP.NET Core `IActionResult` types (`Ok`, `BadRequest`, `NotFound`, `UnprocessableEntity`).
- **Background Jobs / Middleware:** Standardize error reporting and diagnostics.
- **CQRS Handlers:** Provide explicit results for predictable state and data flows.
- **CI/CD Integration:** Use structured errors for automated validation and reporting.

---

## 🏛️ Design Philosophy

- **Composition Over Inheritance:** Compose results and error info rather than subclassing.
- **Status Encapsulation:** Use `IResultStatus` and `ErrorInfo` for rich, extensible outcome details.
- **Exception Separation:** Prefer explicit result passing; reserve exceptions for critical failures.
- **Immutability:** All result types are immutable, ensuring thread safety and predictability.

---

## 📚 Advanced Topics

- **Custom Status:** Implement `IResultStatus` for domain-specific states.
- **ASP.NET Core Integration:** Use provided extensions for mapping to `IActionResult`.
- **Serialization:** Fully compatible with `System.Text.Json` and custom converters.
- **Async Operations:** Use async `Bind` and `Map` for functional composition.
- **Result Value Access:** Use `GetValueOrThrow`, `GetValueOrDefault` for safe value extraction.

For more, see the [Wiki](https://github.com/ulfbou/Zentient.Results/wiki).

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING.md](https://github.com/ulfbou/Zentient.Results/blob/main/CONTRIBUTING.md) for details.

---

## 📄 License

Licensed under the [MIT License](https://github.com/ulfbou/Zentient.Results/blob/main/LICENSE).

---

## ❓ Support

- [GitHub Issues](https://github.com/ulfbou/Zentient.Results/issues) — for bugs and feature requests.
- [GitHub Discussions](https://github.com/ulfbou/Zentient.Results/discussions) — for community Q&A and feedback.
