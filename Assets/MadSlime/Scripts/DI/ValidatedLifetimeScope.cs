using System;
using VContainer.Unity;

namespace DI
{
    public abstract class ValidatedLifetimeScope : LifetimeScope
    {
        protected void ValidateAssigned(object dependency, string fieldName)
        {
            if (dependency == null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name}: '{fieldName}' is not assigned. Select the scope object in the scene "
                        + "and drag the missing reference.");
            }
        }
    }
}
