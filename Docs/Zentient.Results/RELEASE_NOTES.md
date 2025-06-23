# Zentient.Results Release Notes

This document provides a summary of the latest updates and significant changes to the `Zentient.Results` library. For a complete and detailed history of all versions, please refer to the [Zentient.Results Changelog](CHANGELOG.md).

---

## **Version 0.4.1 - (June 23, 2025)**

This is a patch release for `Zentient.Results` focused on improving integration and compatibility within the Zentient ecosystem.

**Key Update:**

* **Expanded Internal Visibility:** Version 0.4.1 introduces `InternalsVisibleTo` attributes for `Zentient.Endpoints`, `Zentient.Telemetry`, and other related libraries. This change resolves internal access issues encountered when consuming `Zentient.Results 0.4.0` as a NuGet package, ensuring smoother development and testing workflows across your projects.

**Important Notes:**

* **No New Breaking Changes:** This release introduces no new breaking changes to the public API beyond those already present in version 0.4.0.
* If you are upgrading from versions prior to 0.4.0, please consult the [Changelog](CHANGELOG.md) for details on the breaking changes introduced in 0.4.0 (e.g., `Result` and `Result<T>` are now sealed classes, `IResult.Error` renamed to `ErrorMessage`).

---

For a comprehensive list of all changes, features, and breaking changes across all versions, please see the [full Changelog](CHANGELOG.md).
