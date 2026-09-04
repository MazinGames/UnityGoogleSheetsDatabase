using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MazinGames.GoogleSheetsDatabase
{
    public static class Utilities
    {
        public static readonly string[] TrueOptions =
        {
            "true", "yes"
        };

        private static readonly string[] FalseOptions =
        {
            "false", "no"
        };

        public static string[] Split(string line)
        {
            var isInsideQuotes = false;
            var result = new List<string>();

            var temp = string.Empty;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    isInsideQuotes = !isInsideQuotes;

                    if (i == line.Length - 1)
                    {
                        result.Add(temp);
                    }
                }
                else
                {
                    if (!isInsideQuotes && line[i] == ',')
                    {
                        result.Add(temp);
                        temp = string.Empty;
                    }
                    else
                    {
                        temp += line[i];
                    }
                }
            }

            return result.ToArray();
        }

        public static object Parse(string s, Type type)
        {
            return Parse(s, type, out var error);
        }

        private static object Parse(string s, Type type, out bool error)
        {
            error = false;

            if (type == typeof(string))
            {
                return s;
            }

            if (type == typeof(int))
            {
                return ParseInt(s, out error);
            }

            if (type == typeof(float))
            {
                return ParseFloat(s, out error);
            }

            if (type == typeof(double))
            {
                return ParseDouble(s, out error);
            }

            if (type == typeof(bool))
            {
                return ParseBool(s, out error);
            }

            if (type == typeof(List<int>))
            {
                return ParseList(s, value =>
                {
                    var result = ParseInt(value, out var error);
                    return (result, error);
                }, out error);
            }

            if (type == typeof(List<float>))
            {
                return ParseList(s, value =>
                {
                    var result = ParseFloat(value, out var error);
                    return (result, error);
                }, out error);
            }

            if (type == typeof(List<double>))
            {
                return ParseList(s, value =>
                {
                    var result = ParseDouble(value, out var error);
                    return (result, error);
                }, out error);
            }
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                if (elementType.IsEnum)
                {
                    return ParseEnumList(s, elementType, out error);
                }
            }

            if (type.IsEnum)
            {
                object result;
                try
                {
                    result = Enum.Parse(type, s, true);
                }
                catch (ArgumentException)
                {
                    result = default;
                    error = false;
                }

                return result;
            }

            return default;
        }

        private static object ParseEnumList(string s, Type enumType, out bool error)
        {
            error = false;

            var items = s.Split(',');
            var listType = typeof(List<>).MakeGenericType(enumType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var item in items)
            {
                var trimmed = item.Trim();
                try
                {
                    if (!Enum.IsDefined(enumType, trimmed))
                    {
                        // This only checks against names. If using numeric values, you’d want to handle that differently.
                        Console.WriteLine($"'{trimmed}' is not a valid value for enum {enumType.Name}.");
                        error = true;
                        return null;
                    }

                    var parsed = Enum.Parse(enumType, trimmed, true);
                    list.Add(parsed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to parse '{trimmed}' into {enumType.Name} enum. Exception: {ex.Message}");
                    error = true;
                    return null;
                }
            }

            return list;
        }

        private static int ParseInt(string s, out bool error)
        {
            error = !int.TryParse(s, out var result);

            if (error)
            {
                Debug.LogWarning($"Error at parsing '{s}' to Integer");
            }

            return error
                ? 0
                : result;
        }

        private static bool ParseBool(string s, out bool error)
        {
            s = s.ToLower();
            error = false;

            for (var i = 0; i < TrueOptions.Length; i++)
            {
                if (s == TrueOptions[i])
                {
                    return true;
                }

                if (s == FalseOptions[i])
                {
                    return false;
                }
            }

            error = true;
            return false;
        }

        private static float ParseFloat(string s, out bool error)
        {
            error = !float.TryParse(
                s.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var result);
            return result;
        }

        private static double ParseDouble(string s, out bool error)
        {
            error = !double.TryParse(
                s.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var result);
            return result;
        }
        
        private static List<T> ParseList<T>(string s, Func<string, (T value, bool error)> parser, out bool error)
        {
            var list = new List<T>();
            var stringArray = s.Split(',');
            error = false;

            foreach (var value in stringArray)
            {
                var (result, itemError) = parser(value.Trim());

                if (itemError)
                {
                    error = true;
                }
                else
                {
                    list.Add(result);
                }
            }

            return list;
        }
    }
}