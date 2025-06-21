// <copyright file="EnumExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Utilities
{
    /// <summary>Provides extension methods for working with enums.</summary>
    public static class EnumExtensions
    {
        /// <summary>Converts an enum value to its string representation.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>The string representation of the enum value.</returns>
        public static string ToString<T>(this T value) where T : Enum
        {
            return value.ToString();
        }

        // Validate that a given value is defined in the enum type.

        /// <summary>Checks if the specified enum value is defined in the enum type.</summary>
        /// typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <returns><see cref="true"/> if the value is defined in the enum type; otherwise, <see cref="false"/>.</returns>
        public static bool IsDefined<T>(this T value) where T : Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }
    }
}
