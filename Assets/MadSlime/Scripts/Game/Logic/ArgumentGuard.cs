using System;

namespace Game
{
    public static class ArgumentGuard
    {
        public static void CheckNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
