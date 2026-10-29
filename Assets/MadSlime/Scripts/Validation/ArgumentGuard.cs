using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Validation
{
    public static class ArgumentGuard
    {
        public static void CheckAssigned(object value, string ownerName, string memberName)
        {
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: '{memberName}' is missing. "
                    + "Assign it in the Inspector or register it in the scene scope.");
            }
        }

        public static void CheckNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void CheckPositive(float value, string paramName)
        {
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
            }
        }

        public static void CheckNonNegative(float value, string paramName)
        {
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be nonnegative.");
            }
        }

        public static void CheckRange(float value, float low, float high, string paramName)
        {
            if (value < low || value > high)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {low} and {high}.");
            }
        }

        public static void CheckPositive(float value, string ownerName, string memberName)
        {
            if (value <= 0f)
            {
                throw new InvalidOperationException($"{ownerName}: '{memberName}' must be positive, got {value}.");
            }
        }

        public static void CheckOrder(float low, float high, string ownerName, string memberName)
        {
            if (low > high)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: '{memberName}' minimum {low} exceeds maximum {high}.");
            }
        }

        public static void CheckPositive(int value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
            }
        }

        public static void CheckNonNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be nonnegative.");
            }
        }

        public static void CheckRange(int value, int low, int high, string paramName)
        {
            if (value < low || value > high)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {low} and {high}.");
            }
        }

        public static void CheckPositive(int value, string ownerName, string memberName)
        {
            if (value <= 0)
            {
                throw new InvalidOperationException($"{ownerName}: '{memberName}' must be positive, got {value}.");
            }
        }

        public static void CheckOrder(int low, int high, string ownerName, string memberName)
        {
            if (low > high)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: '{memberName}' minimum {low} exceeds maximum {high}.");
            }
        }

        public static void CheckNotEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value must not be null or empty.", paramName);
            }
        }

        public static void CheckNotEmpty<T>(IReadOnlyCollection<T> values, string ownerName, string memberName)
        {
            if (values == null || values.Count == 0)
            {
                throw new InvalidOperationException($"{ownerName}: '{memberName}' must contain at least one element.");
            }
        }

        public static void CheckNotEmpty(AnimationCurve curve, string ownerName, string memberName)
        {
            if (curve == null || curve.length == 0)
            {
                throw new InvalidOperationException($"{ownerName}: '{memberName}' must contain at least one keyframe.");
            }
        }
    }
}
