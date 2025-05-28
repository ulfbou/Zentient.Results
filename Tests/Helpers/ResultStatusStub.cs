namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResultStatus"/> for testing purposes.</summary>
    internal class ResultStatusStub : IResultStatus
    {
        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>Initializes a new instance of the <see cref="ResultStatusStub"/> class with a specific code and description.</summary>
        /// <param name="code">The integer code representing the status (e.g., 200, 400, 404).</param>
        /// <param name="description">A human-readable description for the result status (e.g., "OK", "Bad Request", "Not Found").</param>
        public ResultStatusStub(int code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
