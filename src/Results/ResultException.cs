// <copyright file="ResultException.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Represents an exception thrown when an <see cref="IResult"/> indicates a failure.
    /// It encapsulates the <see cref="ErrorInfo"/> list associated with the failure.
    /// </summary>
    [Serializable]
    public class ResultException : Exception
    {
        /// <summary>Gets a read-only list of errors associated with the result failure.</summary>
        public IReadOnlyList<ErrorInfo> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class
        /// with a default message that includes the error details.
        /// </summary>
        /// <param name="errors">A read-only list of <see cref="ErrorInfo"/> instances that caused the failure.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <c>null</c>.</exception>
        public ResultException(IReadOnlyList<ErrorInfo> errors)
            : base("One or more errors occurred: " + string.Join("; ", (errors ?? throw new ArgumentNullException(nameof(errors))).Select(e => e.Message)))
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errors">A read-only list of <see cref="ErrorInfo"/> instances that caused the failure.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <c>null</c>.</exception>
        public ResultException(string message, IReadOnlyList<ErrorInfo> errors)
            : base(message)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing"/> in Visual Basic) if no inner exception is specified.</param>
        /// <param name="errors">A read-only list of <see cref="ErrorInfo"/> instances that caused the failure.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <c>null</c>.</exception>
        public ResultException(string message, Exception innerException, IReadOnlyList<ErrorInfo> errors)
            : base(message, innerException)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
