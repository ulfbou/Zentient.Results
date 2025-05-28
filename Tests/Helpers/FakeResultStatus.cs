namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// Mock implementation of <see cref="IResultStatus"/> for testing purposes.
    /// This class provides a simple way to create result statuses with a code and description, in a test environment.
    /// </summary>
    internal class FakeResultStatus : IResultStatus
    {
        /// <inheritdoc />
        public FakeResultStatus(int code, string desc) { Code = code; Description = desc; }

        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        public string Description { get; }
    }
}
