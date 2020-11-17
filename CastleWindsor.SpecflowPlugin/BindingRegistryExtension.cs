namespace CastleWindsor.SpecflowPlugin
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TechTalk.SpecFlow.Bindings;
    using TechTalk.SpecFlow.Bindings.Reflection;

    internal static class BindingRegistryExtension
    {
        public static IEnumerable<IBindingType> GetBindingTypes(this IBindingRegistry bindingRegistry)
        {
            return bindingRegistry.GetStepDefinitions()
                .Concat(bindingRegistry.GetHooks().Cast<IBinding>())
                .Concat(bindingRegistry.GetStepTransformations())
                .Select(bng => bng.Method.Type)
                .Distinct();
        }

        public static IEnumerable<Assembly> GetBindingAssemblies(this IBindingRegistry bindingRegistry)
        {
            return bindingRegistry.GetBindingTypes()
                .OfType<RuntimeBindingType>()
                .Select(typ => typ.Type.Assembly)
                .Distinct();
        }
    }
}