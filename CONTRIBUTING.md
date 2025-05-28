# Contributing to Zentient.Results

We welcome and appreciate your contributions to Zentient.Results\! By contributing, you help us improve this library for the entire .NET community. This document outlines the guidelines and processes for contributing to the project.

## Code of Conduct

To ensure a welcoming and inclusive environment, all contributors are expected to adhere to our [Code of Conduct](CODE_OF_CONDUCT.md) (if applicable, otherwise state: "We expect all contributors to adhere to a high standard of respectful and professional conduct."). Please review it before contributing.

## How Can I Contribute?

There are several ways you can contribute to Zentient.Results:

### 1\. Reporting Bugs

If you find a bug, please open an issue on our [GitHub Issues page](https://github.com/ulfbou/Zentient.Results/issues). When reporting a bug, please include:

  * A clear and concise description of the bug.
  * Steps to reproduce the behavior.
  * Expected behavior.
  * Actual behavior.
  * Screenshots or code snippets if helpful.
  * Your .NET version and operating system.

### 2. Suggesting Enhancements

Do you have an idea for a new feature or an improvement to an existing one? We'd love to hear it\! Please open an issue on our [GitHub Issues page](https://github.com/ulfbou/Zentient.Results/issues) and describe your suggestion. Include:

  * A clear and concise description of the proposed enhancement.
  * The problem it solves or the benefit it provides.
  * Any potential alternatives or considerations.

### 3. Writing Code

We welcome contributions of code for bug fixes, new features, or refactorings. Before starting any significant work, please:

  * **Check existing issues:** See if your contribution is already being discussed or worked on.
  * **Open an issue:** For new features or major changes, open an issue first to discuss your idea with the maintainers. This helps ensure alignment and avoids duplicate efforts.
  * **Fork the repository:** Create a fork of the `Zentient.Results` repository on GitHub.

### 4. Improving Documentation

High-quality documentation is crucial. If you find typos, inaccuracies, or areas that could be explained more clearly in the README, code comments, or the [Wiki](https://github.com/ulfbou/Zentient.Results/wiki), please consider contributing. You can submit a pull request with your changes or open an issue.

## Getting Started with Development

To set up your local development environment:

1.  **Prerequisites:** Ensure you have the .NET SDK (version 6.0 or newer) installed. We recommend using the latest stable version of .NET 9.0.
2.  **Clone the Repository:**
    ```bash
    git clone https://github.com/ulfbou/Zentient.Results.git
    cd Zentient.Results
    ```
3.  **Build the Project:**
    ```bash
    dotnet build
    ```
4.  **Run Tests:**
    ```bash
    dotnet test
    ```

## Your First Code Contribution

Once you've set up your development environment:

1.  **Create a New Branch:**
    For features: `git checkout -b feature/your-feature-name`
    For bug fixes: `git checkout -b bugfix/issue-number-short-description`
2.  **Make Your Changes:** Implement your feature or bug fix.
3.  **Adhere to Coding Style:**
      * Follow standard .NET coding conventions.
      * We use an `.editorconfig` file to help maintain consistent formatting. Your IDE should pick this up automatically.
      * Ensure your code is clean, readable, and well-commented where necessary.
4.  **Write Tests:**
      * All new features must be accompanied by appropriate unit tests.
      * Bug fixes should include a regression test that fails without your fix and passes with it.
      * Ensure all existing tests pass after your changes.
5.  **Commit Your Changes:**
    Write clear, concise commit messages. We encourage using [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) (e.g., `feat: add new result type`, `fix: resolve serialization issue`).
    ```bash
    git add .
    git commit -m "feat: descriptive commit message"
    ```
6.  **Push to Your Fork:**
    ```bash
    git push origin feature/your-feature-name
    ```
7.  **Create a Pull Request (PR):**
      * Go to the `Zentient.Results` GitHub repository and you should see a prompt to create a new pull request from your pushed branch.
      * Provide a clear title and detailed description for your PR.
      * Reference any related issues (e.g., `Closes #123` or `Fixes #123`).
      * Explain the changes you've made and why they are necessary.
      * Ensure all checks (tests, linting) pass.
      * Be prepared for feedback and discussions during the review process.

## License

By contributing to Zentient.Results, you agree that your contributions will be licensed under the [MIT License](https://github.com/ulfbou/Zentient.Results/blob/main/LICENSE).

## Support & Contact

If you have any questions or need clarification on the contributing process, please don't hesitate to open an issue on our [GitHub Issues page](https://github.com/ulfbou/Zentient.Results/issues).
