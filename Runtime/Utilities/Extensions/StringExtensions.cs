using System;
using System.Text;
using System.Text.RegularExpressions;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for string.
    /// </summary>
    public static class StringExtensions
    {
        #region Validation

        /// <summary>
        /// Checks if the string is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Checks if the string is null or whitespace.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Returns the string or a default value if null/empty.
        /// </summary>
        public static string OrDefault(this string s, string defaultValue)
        {
            return string.IsNullOrEmpty(s) ? defaultValue : s;
        }

        /// <summary>
        /// Returns null if the string is empty.
        /// </summary>
        public static string NullIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        #endregion

        #region Case Conversion

        /// <summary>
        /// Converts to Title Case.
        /// </summary>
        public static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        /// <summary>
        /// Converts to camelCase.
        /// </summary>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var words = s.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return s;

            var sb = new StringBuilder();
            sb.Append(words[0].ToLower());

            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    sb.Append(char.ToUpper(words[i][0]));
                    if (words[i].Length > 1)
                        sb.Append(words[i].Substring(1).ToLower());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts to PascalCase.
        /// </summary>
        public static string ToPascalCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var words = s.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    sb.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLower());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts to snake_case.
        /// </summary>
        public static string ToSnakeCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var sb = new StringBuilder();
            sb.Append(char.ToLower(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]))
                {
                    sb.Append('_');
                    sb.Append(char.ToLower(s[i]));
                }
                else if (s[i] == ' ' || s[i] == '-')
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(s[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts to kebab-case.
        /// </summary>
        public static string ToKebabCase(this string s)
        {
            return s.ToSnakeCase().Replace('_', '-');
        }

        /// <summary>
        /// Converts camelCase/PascalCase to words with spaces.
        /// </summary>
        public static string ToWords(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Truncates the string to a maximum length.
        /// </summary>
        public static string Truncate(this string s, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLength) return s;
            return s.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Removes all whitespace from the string.
        /// </summary>
        public static string RemoveWhitespace(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Regex.Replace(s, @"\s+", "");
        }

        /// <summary>
        /// Removes duplicate whitespace.
        /// </summary>
        public static string CollapseWhitespace(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Regex.Replace(s, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Reverses the string.
        /// </summary>
        public static string Reverse(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var chars = s.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Repeats the string n times.
        /// </summary>
        public static string Repeat(this string s, int count)
        {
            if (string.IsNullOrEmpty(s) || count <= 0) return string.Empty;
            var sb = new StringBuilder(s.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Pads the string to center it.
        /// </summary>
        public static string PadCenter(this string s, int totalWidth, char paddingChar = ' ')
        {
            if (string.IsNullOrEmpty(s)) return new string(paddingChar, totalWidth);
            if (s.Length >= totalWidth) return s;

            int leftPad = (totalWidth - s.Length) / 2;
            int rightPad = totalWidth - s.Length - leftPad;
            return new string(paddingChar, leftPad) + s + new string(paddingChar, rightPad);
        }

        #endregion

        #region Contains & Search

        /// <summary>
        /// Checks if the string contains another string (case-insensitive).
        /// </summary>
        public static bool ContainsIgnoreCase(this string s, string value)
        {
            if (s == null || value == null) return false;
            return s.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Checks if the string starts with another string (case-insensitive).
        /// </summary>
        public static bool StartsWithIgnoreCase(this string s, string value)
        {
            if (s == null || value == null) return false;
            return s.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the string ends with another string (case-insensitive).
        /// </summary>
        public static bool EndsWithIgnoreCase(this string s, string value)
        {
            if (s == null || value == null) return false;
            return s.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the string equals another string (case-insensitive).
        /// </summary>
        public static bool EqualsIgnoreCase(this string s, string value)
        {
            return string.Equals(s, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Counts occurrences of a substring.
        /// </summary>
        public static int CountOccurrences(this string s, string value)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(value)) return 0;

            int count = 0;
            int index = 0;
            while ((index = s.IndexOf(value, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += value.Length;
            }
            return count;
        }

        #endregion

        #region Numeric

        /// <summary>
        /// Checks if the string is a valid integer.
        /// </summary>
        public static bool IsInteger(this string s)
        {
            return int.TryParse(s, out _);
        }

        /// <summary>
        /// Checks if the string is a valid number (float).
        /// </summary>
        public static bool IsNumeric(this string s)
        {
            return float.TryParse(s, out _);
        }

        /// <summary>
        /// Parses as int or returns default.
        /// </summary>
        public static int ToIntOrDefault(this string s, int defaultValue = 0)
        {
            return int.TryParse(s, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Parses as float or returns default.
        /// </summary>
        public static float ToFloatOrDefault(this string s, float defaultValue = 0f)
        {
            return float.TryParse(s, out float result) ? result : defaultValue;
        }

        /// <summary>
        /// Parses as bool or returns default.
        /// </summary>
        public static bool ToBoolOrDefault(this string s, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(s)) return defaultValue;

            s = s.ToLower().Trim();
            if (s == "true" || s == "1" || s == "yes" || s == "on") return true;
            if (s == "false" || s == "0" || s == "no" || s == "off") return false;
            return defaultValue;
        }

        #endregion

        #region Format

        /// <summary>
        /// Formats a file size in bytes to human-readable format.
        /// </summary>
        public static string ToFileSizeString(this long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Formats a number with thousands separators.
        /// </summary>
        public static string ToFormattedNumber(this int number)
        {
            return number.ToString("N0");
        }

        #endregion
    }
}
