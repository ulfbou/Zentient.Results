# **Zentient.Results 0.4.2**

We are releasing **Zentient.Results, version 0.4.2**, a patch update focused on enhancing the consistency, robustness, and internal clarity of `Result` object creation. This release builds upon previous versions by refining how errors and messages are handled within the core `Result` types.

## **What's New in 0.4.2**

This version introduces the following key improvements:

  * **Consistent Error and Message Handling**:
      * The `Result` constructor and `Failure` factory methods have been enhanced to ensure that `ErrorInfo.Message` values are consistently propagated to the `Result.Messages` collection. This provides a more unified and predictable view of both informational and error messages within a `Result` object.
      * Internal handling of error and message collections has been refined for improved robustness.
  * **Streamlined Result Factory Methods**:
      * The static factory methods for common failure scenarios (e.g., `NotFound`, `Unauthorized`, `Validation`, `Conflict`) have been refactored. They now leverage `Zentient.Results.Constants` for standardized default messages and error codes, promoting greater consistency across various failure types.
      * The internal creation of `Result` instances in these methods has been streamlined for reduced redundancy and improved clarity.
  * **Minor Codebase Improvements**:
      * Removed an unnecessary byte order mark (BOM) from the `Result.cs` file, addressing a minor encoding detail.

## **Important Notes**

  * **Patch Release:** This is a **patch version** and introduces **no new breaking changes** to the public API of `Zentient.Results` beyond those already present in version 0.4.0.
  * **0.4.0 Breaking Changes Still Apply:** If you are upgrading from a version older than 0.4.0, please ensure your code has been adjusted for the significant breaking changes introduced in **Zentient.Results 0.4.0** (e.g., `Result` and `Result<T>` as sealed classes, `IResult.Error` renamed to `IResult.ErrorMessage`, `ErrorInfo.Metadata` consolidation). This 0.4.2 release does not revert or modify those changes.
  * **Feedback Welcome**: Your feedback is crucial for the continued improvement of Zentient.Results. Please report any issues, suggest improvements, or share your usage experiences by opening an issue on our GitHub repository.

## **Installation**

To update or install this version, you can use NuGet:

```bash
dotnet add package Zentient.Results --version 0.4.2
```

Or using the Package Manager Console:

```powershell
Install-Package Zentient.Results -Version 0.4.2
```

**Created:** 2025-06-27 **Version:** 0.4.2
