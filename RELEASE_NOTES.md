# **Zentient.Results 0.4.1**

We are releasing **Zentient.Results, version 0.4.1**, a patch update primarily focused on improving integration and compatibility within the Zentient ecosystem. This release directly addresses challenges faced by dependent libraries when consuming `Zentient.Results 0.4.0` as a NuGet package.

## **What's New in 0.4.1**

This version introduces the following key improvement:

* **Expanded Internal Visibility (`InternalsVisibleTo`):**
    * To ensure smoother integration and enable advanced testing scenarios, `Zentient.Results` now explicitly exposes its internal types and members to designated friend assemblies.
    * This includes the `Zentient.Endpoints`, `Zentient.Telemetry`, and other related projects, which can now seamlessly access necessary internal components without compromising the public API.
    * This change mitigates compilation errors and integration friction previously experienced when `Zentient.Results 0.4.0` was consumed as a NuGet package, where internal access was lost compared to a direct project reference.

## **Important Notes**

* **Patch Release:** This is a **patch version** and introduces **no new breaking changes** to the public API of `Zentient.Results` beyond those already present in version 0.4.0.
* **0.4.0 Breaking Changes Still Apply:** If you are upgrading from a version older than 0.4.0, please ensure your code has been adjusted for the significant breaking changes introduced in **Zentient.Results 0.4.0** (e.g., `Result` and `Result<T>` as sealed classes, `IResult.Error` renamed to `IResult.ErrorMessage`, `ErrorInfo.Metadata` consolidation). This 0.4.1 release does not revert or modify those changes.
* **Feedback Welcome**: Your feedback is crucial for the continued improvement of Zentient.Results. Please report any issues, suggest improvements, or share your usage experiences by opening an issue on our GitHub repository.

## **Installation**

To update or install this version, you can use NuGet:

```bash
dotnet add package Zentient.Results --version 0.4.1
````

Or using the Package Manager Console:

```powershell
Install-Package Zentient.Results -Version 0.4.1
```

**Created:** 2025-06-23 **Version:** 0.4.1
