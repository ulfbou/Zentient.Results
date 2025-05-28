namespace Zentient.Results.AspNetCore.Configuration
{
    public class ZentientProblemDetailsOptions
    {
        /// <summary>
        /// Gets or sets the base URI for custom problem detail types.
        /// This URI should typically point to documentation explaining the error.
        /// Example: "https://yourdomain.com/errors/"
        /// </summary>
        public string ProblemTypeBaseUri { get; set; } = "https://default.com/errors/";
    }
}
