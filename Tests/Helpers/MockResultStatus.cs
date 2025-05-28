namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResultStatus"/> for testing purposes.</summary>
    internal class MockResultStatus : IResultStatus
    {
        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>Initializes a new instance of the <see cref="MockResultStatus"/> class with a specific code and optional description.</summary>
        /// <param name="code">The integer code representing the status (e.g., 200, 400, 404).</param>
        /// <param name="description">An optional human-readable description for the result status (e.g., "OK", "Bad Request", "Not Found").</param>
        public MockResultStatus(int code, string? description = null)
        {
            Code = code;
            Description = description ?? code.ToString();
        }
    }
}
