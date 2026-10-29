using MadSlime.Validation;
using VContainer.Unity;

namespace MadSlime.DI
{
    public abstract class ValidatedLifetimeScope : LifetimeScope
    {
        protected void ValidateAssigned(object dependency, string fieldName)
        {
            ArgumentGuard.CheckAssigned(dependency, name, fieldName);
        }
    }
}
