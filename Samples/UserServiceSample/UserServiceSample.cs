using System;
using System.Collections.Generic;

using Zentient.Results;

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
