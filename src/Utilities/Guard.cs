// <copyright file="Guard.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Generic;

using Zentient.Utilities;

namespace Zentient.Utilities
{
    /// <summary>Provides guard clauses for argument validation.</summary>
    public static class Guard
    {
        /// <summary>Throws an <see cref="ArgumentException"/> if the specified string is null or empty.</summary>
        /// <param name="value">The string value to check.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null or empty.</exception>
        public static void AgainstNullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be null or empty.", parameterName);
            }
        }

        /// <summary>Throws an <see cref="ArgumentException"/> if the specified string is null or whitespace.</summary>
        /// <param name="value">The string value to check.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null or whitespace.</exception>
        public static void AgainstNullOrWhitespace(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be null or whitespace.", parameterName);
            }
        }

        /// <summary>Throws an <see cref="ArgumentNullException"/> if the specified value is null.</summary>
        /// <typeparam name="T">The type of the value to check. Must be a reference type.</typeparam>
        /// <param name="value">The value to check for null.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void AgainstNull<T>(T? value, string parameterName) where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName, $"Argument '{parameterName}' cannot be null.");
            }
        }

        /// <summary>Throws an <see cref="ArgumentException"/> if the specified value is the default value for its type.</summary>
        /// <typeparam name="T">The type of the value to check. Must be a value type.</typeparam>
        /// <param name="value">The value to check for default.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is the default value for its type.</exception>
        public static void AgainstDefault<T>(T value, string parameterName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be the default value.", parameterName);
            }
        }

        /// <summary>Throws an <see cref="ArgumentException"/> if the specified array is null or empty.</summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="arr">The array to check.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        public static void AgainstNullOrEmpty<T>(T[]? arr, string parameterName)
        {
            if (arr is null || arr.Length == 0)
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be null or empty.", parameterName);
            }
        }
    }
}
