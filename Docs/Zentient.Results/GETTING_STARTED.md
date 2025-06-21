Okay, let's refine the "Getting Started" guide to fully align with the 0.4.0 changes, particularly the shift from `struct` to `class` for `Result` types, the enhancements to `ErrorInfo`, and the updated property names.

Here's the updated content for your `Getting_Started.md` file:

-----

# Getting Started with Zentient.Results

This guide will walk you through the process of integrating and effectively using the Zentient.Results library in your .NET projects. Zentient.Results provides a powerful and idiomatic way to handle operation outcomes, promoting explicit success and failure states, and enhancing the robustness and readability of your codebase.

## 1\. Installation

The easiest way to add Zentient.Results to your project is via NuGet Package Manager.

### Using .NET CLI

Open your terminal or command prompt and navigate to your project directory. Then, run the following command:

```bash
dotnet add package Zentient.Results
```

### Using NuGet Package Manager Console

In Visual Studio, open the NuGet Package Manager Console (`Tools` \> `NuGet Package Manager` \> `Package Manager Console`) and execute:

```powershell
Install-Package Zentient.Results
```

### Using Visual Studio's NuGet Package Manager UI

1.  Right-click on your project in the Solution Explorer.
2.  Select "Manage NuGet Packages...".
3.  Go to the "Browse" tab.
4.  Search for `Zentient.Results`.
5.  Select the package and click "Install".

## 2\. Basic Usage

Zentient.Results primarily revolves around the `Result<T>` and `Result` **classes**.

### 2.1 Handling Operations with a Return Value (`Result<T>`)

For operations that produce a value upon success, use `Result<T>`.

#### Success Example

```csharp
using Zentient.Results;
using System;
using System.Linq;
using System.Collections.Generic; // Added for List<ErrorInfo>

public class UserService
{
    public IResult<User> GetUserById(int id)
    {
        // Simulate fetching a user from a database
        if (id > 0)
        {
            var user = new User { Id = id, Name = "John Doe" };
            return Result<User>.Success(user, $"User {user.Name} retrieved successfully.");
        }

        // This path indicates a logical failure, not an exception
        return Result<User>.NotFound(
            new ErrorInfo(ErrorCategory.NotFound, "USER_NOT_FOUND", $"User with ID {id} was not found.")
        );
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// How to consume it:
public class Application
{
    public void Run()
    {
        var userService = new UserService();

        // Successful scenario
        IResult<User> userResult = userService.GetUserById(1);
        if (userResult.IsSuccess)
        {
            Console.WriteLine($"Successfully retrieved user: {userResult.Value.Name}");
            Console.WriteLine($"Status: {userResult.Status.Description}");
            Console.WriteLine($"Message: {userResult.Messages.FirstOrDefault()}");
        }
        else
        {
            Console.WriteLine($"Failed to retrieve user: {userResult.ErrorMessage}"); // Updated property
            Console.WriteLine($"Status: {userResult.Status.Description} ({userResult.Status.Code})");
            foreach (var error in userResult.Errors)
            {
                Console.WriteLine($"Error: [{error.Category}:{error.Code}] {error.Message}");
            }
        }

        Console.WriteLine("\n---");

        // Failure scenario
        IResult<User> failedUserResult = userService.GetUserById(0);
        if (failedUserResult.IsSuccess)
        {
            // This block won't be executed
            Console.WriteLine("Unexpected success!");
        }
        else
        {
            Console.WriteLine($"Failed to retrieve user: {failedUserResult.ErrorMessage}"); // Updated property
            Console.WriteLine($"Status: {failedUserResult.Status.Description} ({failedUserResult.Status.Code})");
            foreach (var error in failedUserResult.Errors)
            {
                Console.WriteLine($"Error: [{error.Category}:{error.Code}] {error.Message}");
            }
        }
    }
}
```

#### Implicit Conversion from `T` to `Result<T>`

For convenience, you can often return `T` directly, and it will be implicitly converted to a successful `Result<T>`.

```csharp
using Zentient.Results;

public class AnotherUserService
{
    public IResult<User> GetUser(int id)
    {
        if (id == 5)
        {
            return new User { Id = 5, Name = "Jane Doe" }; // Implicitly converts to Result<User>.Success(user)
        }
        return Result<User>.NotFound(new ErrorInfo(ErrorCategory.NotFound, "USER_NF", "User not found."));
    }
}
```

### 2.2 Handling Operations Without a Return Value (`Result`)

For operations that don't return a specific value but still indicate success or failure (e.g., a `void` method), use the non-generic `Result` **class**.

```csharp
using Zentient.Results;
using System;
using System.Linq;
using System.Collections.Generic; // Added for List<ErrorInfo>

public class DataWriter
{
    public IResult WriteData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return Result.Validation(
                new ErrorInfo(ErrorCategory.Validation, "DATA_EMPTY", "Data cannot be empty.")
            );
        }

        // Simulate writing data
        if (data.Length > 100)
        {
            return Result.Forbidden(
                new ErrorInfo(ErrorCategory.Authorization, "DATA_TOO_LONG", "Data exceeds maximum allowed length.")
            );
        }

        Console.WriteLine($"Data '{data}' written successfully.");
        return Result.Success(ResultStatuses.Accepted, "Data processing initiated.");
    }
}

// How to consume it:
public class AnotherApplication
{
    public void Run()
    {
        var dataWriter = new DataWriter();

        IResult writeResult1 = dataWriter.WriteData("Hello World");
        if (writeResult1.IsSuccess)
        {
            Console.WriteLine($"Operation successful: {writeResult1.Messages.FirstOrDefault()}");
        }
        else
        {
            Console.WriteLine($"Operation failed: {writeResult1.ErrorMessage}"); // Updated property
        }

        Console.WriteLine("\n---");

        IResult writeResult2 = dataWriter.WriteData(string.Empty);
        if (writeResult2.IsSuccess)
        {
            // This block won't be executed
        }
        else
        {
            Console.WriteLine($"Operation failed: {writeResult2.ErrorMessage}"); // Updated property
            Console.WriteLine($"Status: {writeResult2.Status.Description} ({writeResult2.Status.Code})");
        }
    }
}
```

#### Implicit Conversion from `ErrorInfo` to `Result`

An `ErrorInfo` can be implicitly converted to a failed `Result`.

```csharp
using Zentient.Results;

public IResult DeleteItem(int id)
{
    if (id <= 0)
    {
        // Implicitly converts to a failed Result with a Validation status
        return ErrorInfo.Validation("INVALID_ID", "Item ID must be positive.");
    }
    // Perform deletion...
    return Result.NoContent("Item deleted.");
}
```

## 3\. Working with Errors (`ErrorInfo` and `ErrorCategory`)

`ErrorInfo` is a central piece for detailed error reporting. It is now a `sealed class` that provides rich context for failures.

```csharp
using Zentient.Results;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable; // Added for ImmutableDictionary/List

public class PaymentProcessor
{
    public IResult ProcessPayment(decimal amount)
    {
        if (amount <= 0)
        {
            // Using ErrorInfo.Validation factory method and 'metadata'
            return Result.Validation(
                ErrorInfo.Validation(
                    "AMOUNT_INVALID",
                    "Payment amount must be greater than zero.",
                    detail: $"The submitted amount was {amount}.", // New 'detail' property
                    metadata: ImmutableDictionary<string, object?>.Empty.Add("SubmittedAmount", amount)
                )
            );
        }

        if (amount > 1000)
        {
            // Simulating a business rule conflict
            return Result.Conflict(
                new ErrorInfo(ErrorCategory.Conflict, "LARGE_TRANSACTION", "Payment exceeds single transaction limit.")
            );
        }

        // Simulate a network error
        if (new Random().Next(0, 5) == 0) // 20% chance of network error
        {
            return Result.Failure(
                new ErrorInfo(ErrorCategory.Network, "PAYMENT_GATEWAY_UNAVAILABLE", "Could not connect to payment gateway.",
                    detail: "The external payment service responded with a connection timeout."),
                ResultStatuses.ServiceUnavailable
            );
        }

        // Simulating multiple validation errors
        if (amount == 42)
        {
             return Result.Validation(new[]
            {
                ErrorInfo.Validation("MAGIC_NUMBER_ERROR", "42 is not allowed."),
                new ErrorInfo(ErrorCategory.General, "INVALID_VALUE", "This amount has a special meaning and cannot be processed.")
            });
        }

        Console.WriteLine($"Processing payment of {amount:C}...");
        return Result.Success("Payment processed successfully.");
    }
}

// Consuming multiple errors
public class PaymentApplication
{
    public void Run()
    {
        var processor = new PaymentProcessor();
        var result = processor.ProcessPayment(42);

        if (result.IsFailure)
        {
            Console.WriteLine($"Payment Failed: {result.ErrorMessage}"); // Updated property
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - [{error.Category}:{error.Code}] {error.Message}");
                if (!string.IsNullOrEmpty(error.Detail)) // Check for new 'detail' property
                {
                    Console.WriteLine($"    Detail: {error.Detail}");
                }
                if (error.Metadata != null && error.Metadata.Any()) // Check for new 'metadata' property
                {
                    Console.WriteLine($"    Metadata:");
                    foreach (var kvp in error.Metadata)
                    {
                        Console.WriteLine($"      {kvp.Key}: {kvp.Value}");
                    }
                }
            }
        }
    }
}
```

## 4\. Result Statuses (`ResultStatuses`)

`ResultStatuses` provides a convenient collection of pre-defined `IResultStatus` instances, aligning with common HTTP status codes.

```csharp
using Zentient.Results;
using System.Collections.Generic; // Added for List<ErrorInfo>

public IResult ValidateApiKey(string apiKey)
{
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return Result.Unauthorized(); // Uses ResultStatuses.Unauthorized (401)
    }
    if (apiKey != "VALID_KEY")
    {
        return Result.Forbidden(
            new ErrorInfo(ErrorCategory.Authorization, "INVALID_API_KEY", "Provided API key is not valid.")
        ); // Uses ResultStatuses.Forbidden (403)
    }
    return Result.Success(ResultStatuses.Accepted, "API Key validated.");
}

// You can also create custom statuses using ResultStatus.Custom or ResultStatuses.GetStatus:
public IResult HandleRateLimit()
{
    // Using a predefined HTTP-aligned status from ResultStatuses
    return Result.Failure(
        ErrorInfo.General("RATE_LIMIT_EXCEEDED", "You have exceeded the request limit."),
        ResultStatuses.TooManyRequests // Now directly available in ResultStatuses
    );
    // Alternatively, define a truly custom one:
    // return Result.Failure(
    //     ErrorInfo.General("CUSTOM_LIMIT_EXCEEDED", "Your custom limit was exceeded."),
    //     ResultStatus.Custom(9000, "Custom Rate Limited Status")
    // );
}
```

## 5\. Functional Operations (`Map`, `Bind`, `Then`, `OnSuccess`, `OnFailure`, `Tap`)

These methods allow for powerful chaining and handling of `Result<T>` and `Result` instances, especially useful in a functional programming style.

```csharp
using Zentient.Results;
using System;
using System.Linq;
using System.Threading.Tasks; // Added for async operations

public class DataProcessor
{
    public IResult<string> FetchData(int id)
    {
        if (id == 1) return Result<string>.Success("Raw data from ID 1");
        return Result<string>.NotFound(new ErrorInfo(ErrorCategory.NotFound, "DATA_NF", "Data not found."));
    }

    public IResult<int> ParseData(string rawData)
    {
        if (int.TryParse(rawData.Replace("Raw data from ID ", ""), out int parsedId))
        {
            return Result<int>.Success(parsedId);
        }
        return Result<int>.Validation(new ErrorInfo(ErrorCategory.Validation, "PARSE_FAIL", "Could not parse data."));
    }

    public IResult<bool> ProcessNumericData(int numericData)
    {
        if (numericData % 2 == 0)
        {
            return Result<bool>.Success(true, "Data is even.");
        }
        return Result<bool>.Validation(new ErrorInfo(ErrorCategory.Validation, "ODD_DATA", "Data is odd and cannot be processed."));
    }

    // Example of an async operation returning an IResult<T>
    public async Task<IResult<string>> FetchDataAsync(int id)
    {
        await Task.Delay(100); // Simulate async work
        if (id == 100) return Result<string>.Success("Async data from ID 100");
        return Result<string>.NotFound(new ErrorInfo(ErrorCategory.NotFound, "ASYNC_DATA_NF", "Async data not found."));
    }
}

public class FunctionalApplication
{
    public async Task Run() // Made async to demonstrate async Bind
    {
        var processor = new DataProcessor();

        // Example using Map and Bind (synchronous chain)
        IResult<bool> finalResult = processor.FetchData(1)
            .Map(rawData => rawData.ToUpper()) // Map transforms the success value
            .Bind(upperCaseData => processor.ParseData(upperCaseData)) // Bind chains operations that return Results
            .Bind(parsedId => processor.ProcessNumericData(parsedId));

        finalResult
            .OnSuccess(isProcessed => Console.WriteLine($"Processing successful! Is data processed: {isProcessed}"))
            .OnFailure(errors =>
            {
                Console.WriteLine("Processing failed!");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  Error: {error.Message}");
                }
            })
            .Tap(isProcessed =>
            {
                if (isProcessed) Console.WriteLine("Tapped: Data was indeed processed.");
            });

        Console.WriteLine("\n---");

        // Example using async Bind
        IResult<string> asyncChainResult = await processor.FetchDataAsync(100)
            .Bind(asyncData => Task.FromResult(processor.ParseData(asyncData.Replace("Async data from ID ", "")))); // Bind with an async function

        asyncChainResult
            .OnSuccess(id => Console.WriteLine($"Async chain successful! Parsed ID: {id}"))
            .OnFailure(errors => Console.WriteLine($"Async chain failed: {errors.First().Message}"));

        Console.WriteLine("\n---");

        // Example using Match
        string output = processor.FetchData(2) // This will fail
            .Match(
                onSuccess: data => $"Successfully fetched and processed: {data}",
                onFailure: errors => $"Failed to fetch data: {errors.First().Message}"
            );
        Console.WriteLine(output);
    }
}
```

## 6\. Value Access Strategies (`GetValueOrThrow`, `GetValueOrDefault`)

Choose the appropriate method for retrieving the value based on your error handling philosophy.

```csharp
using Zentient.Results;
using System;
using System.Linq; // Added for LINQ extensions on Errors

public class ItemService
{
    public IResult<Item> GetItem(int id)
    {
        if (id == 10)
        {
            return Result<Item>.Success(new Item { Id = 10, Name = "Special Item" });
        }
        return Result<Item>.NotFound(ErrorInfo.NotFound("ITEM_MISSING", "Item not found."));
    }
}

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ValueAccessApplication
{
    public void Run()
    {
        var itemService = new ItemService();

        // Using GetValueOrThrow()
        IResult<Item> successResult = itemService.GetItem(10);
        try
        {
            Item item = successResult.GetValueOrThrow();
            Console.WriteLine($"Found item: {item.Name}");

            // Example of throwing custom exception with message
            Item item2 = successResult.GetValueOrThrow("Could not retrieve the item due to a specific issue.");
            Console.WriteLine($"Found item (custom message): {item2.Name}");

            // Example of throwing custom exception using a factory
            Item item3 = successResult.GetValueOrThrow(() => new CustomItemNotFoundException("Item was truly not found!"));
            Console.WriteLine($"Found item (custom exception factory): {item3.Name}");
        }
        catch (ResultException ex) // Catches the specific ResultException thrown by GetValueOrThrow
        {
            Console.WriteLine($"Error getting item (GetValueOrThrow): {ex.Message}");
            Console.WriteLine($"Errors: {string.Join("; ", ex.Errors.Select(e => e.Message))}");
        }
        catch (CustomItemNotFoundException ex)
        {
            Console.WriteLine($"Custom error getting item: {ex.Message}");
        }


        IResult<Item> failureResult = itemService.GetItem(99);
        try
        {
            Item item = failureResult.GetValueOrThrow(); // Will throw ResultException
            Console.WriteLine($"Found item: {item.Name}"); // This line won't be reached
        }
        catch (ResultException ex) // Catch the specific exception
        {
            Console.WriteLine($"Error getting item (GetValueOrThrow from failure): {ex.Message}");
            Console.WriteLine($"Errors: {string.Join("; ", ex.Errors.Select(e => e.Message))}");
        }

        // Using GetValueOrDefault()
        Item defaultItem = new Item { Id = 0, Name = "Default Item" };
        Item retrievedItem = failureResult.GetValueOrDefault(defaultItem);
        Console.WriteLine($"Retrieved item (GetValueOrDefault): {retrievedItem.Name}"); // Will print "Default Item"

        Item successfulItem = successResult.GetValueOrDefault(defaultItem);
        Console.WriteLine($"Retrieved item (GetValueOrDefault success): {successfulItem.Name}"); // Will print "Special Item"
    }
}

public class CustomItemNotFoundException : Exception
{
    public CustomItemNotFoundException(string message) : base(message) { }
    public CustomItemNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
```

## 7\. JSON Serialization

Zentient.Results provides built-in JSON serialization support using `System.Text.Json` via `ResultJsonConverter`.

To enable serialization, you need to add `ResultJsonConverter` to your `JsonSerializerOptions`.

```csharp
using Zentient.Results;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

public class SerializationExample
{
    public static void Run()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ResultJsonConverter() },
            // Optional: Configure property naming policy for camelCase in JSON
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Example 1: Successful Result<T>
        IResult<User> successResult = Result<User>.Success(
                new User { Id = 1, Name = "Alice" },
                "User data fetched."
            );
        string jsonSuccess = JsonSerializer.Serialize(successResult, options);
        Console.WriteLine("Successful Result<User> JSON:");
        Console.WriteLine(jsonSuccess);

        var deserializedSuccess = JsonSerializer.Deserialize<Result<User>>(jsonSuccess, options);
        Console.WriteLine($"Deserialized success: IsSuccess={deserializedSuccess.IsSuccess}, User Name={deserializedSuccess.Value?.Name}");
        Console.WriteLine($"Deserialized success status: {deserializedSuccess.Status.Description} ({deserializedSuccess.Status.Code})");
        Console.WriteLine($"Deserialized success message: {deserializedSuccess.Messages.FirstOrDefault()}");
        Console.WriteLine("\n---");

        // Example 2: Failed Result (non-generic) with multiple errors, including 'detail' and 'metadata'
        IResult failureResult = Result.Failure(new List<ErrorInfo>
            {
                new ErrorInfo(ErrorCategory.Validation, "INVALID_EMAIL", "Email format is incorrect.", detail: "The provided email 'test@example' does not match the required pattern."),
                new ErrorInfo(ErrorCategory.Security, "PASSWORD_WEAK", "Password does not meet complexity requirements.",
                    metadata: new Dictionary<string, object?> { { "MinLength", 8 }, { "RequiresSymbol", true } }.ToImmutableDictionary()) // Using ImmutableDictionary
            }, ResultStatuses.UnprocessableEntity);

        string jsonFailure = JsonSerializer.Serialize(failureResult, options);
        Console.WriteLine("Failed Result JSON:");
        Console.WriteLine(jsonFailure);

        var deserializedFailure = JsonSerializer.Deserialize<Result>(jsonFailure, options);
        Console.WriteLine($"Deserialized failure: IsFailure={deserializedFailure.IsFailure}, Error={deserializedFailure.ErrorMessage}");
        Console.WriteLine($"Deserialized failure status code: {deserializedFailure.Status.Code}");
        Console.WriteLine($"Deserialized failure errors count: {deserializedFailure.Errors.Count}");
        if (deserializedFailure.Errors.Any())
        {
            Console.WriteLine($"First error code: {deserializedFailure.Errors[0].Code}");
            Console.WriteLine($"First error detail: {deserializedFailure.Errors[0].Detail}");
            if (deserializedFailure.Errors.Count > 1 && deserializedFailure.Errors[1].Metadata.Any())
            {
                Console.WriteLine($"Second error metadata (MinLength): {deserializedFailure.Errors[1].Metadata["minLength"]}"); // Access using camelCase for JSON property
            }
        }
        Console.WriteLine("\n---");

        // Example 3: Failed Result<T> from an Exception
        try
        {
            throw new InvalidOperationException("Something unexpected happened!", new ArgumentNullException("arg"));
        }
        catch (Exception ex)
        {
            IResult<int> exceptionResult = Result<int>.FromException(0, ex, ResultStatuses.InternalServerError);
            string jsonExceptionResult = JsonSerializer.Serialize(exceptionResult, options);
            Console.WriteLine("Failed Result<int> from Exception JSON:");
            Console.WriteLine(jsonExceptionResult);

            var deserializedExceptionResult = JsonSerializer.Deserialize<Result<int>>(jsonExceptionResult, options);
            Console.WriteLine($"Deserialized exception result: IsFailure={deserializedExceptionResult.IsFailure}");
            Console.WriteLine($"Deserialized exception error message: {deserializedExceptionResult.ErrorMessage}");
            if (deserializedExceptionResult.Errors.Any())
            {
                var errorInfo = deserializedExceptionResult.Errors[0];
                Console.WriteLine($"Error code: {errorInfo.Code}");
                Console.WriteLine($"Error message: {errorInfo.Message}");
                Console.WriteLine($"Error detail: {errorInfo.Detail}"); // Will typically be the exception message
                if (errorInfo.Metadata.TryGetValue("exceptionStackTrace", out var stackTrace)) // Access using camelCase for JSON property
                {
                    Console.WriteLine($"Exception StackTrace (partial): {stackTrace?.ToString()?.Substring(0, Math.Min(100, stackTrace.ToString().Length))}...");
                }
            }
        }
    }
}
```
