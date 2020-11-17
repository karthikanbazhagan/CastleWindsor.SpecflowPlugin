namespace CastleWindsor.SpecflowPlugin
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Castle.Windsor;
    using TechTalk.SpecFlow.Bindings;

    internal class ContainerFinder : IContainerFinder
    {
        private readonly IBindingRegistry bindingRegistry;
        private readonly Lazy<Func<IWindsorContainer>> createScenarioContainer;

        public ContainerFinder(IBindingRegistry bindingRegistry)
        {
            this.bindingRegistry = bindingRegistry;
            createScenarioContainer = new Lazy<Func<IWindsorContainer>>(FindCreateScenarioContainer, true);
        }

        public Func<IWindsorContainer> GetCreateScenarioContainer()
        {
            var builder = createScenarioContainer.Value;

            if (builder == null)
            {
                throw new Exception("Unable to find scenario dependencies.  Mark a static method that returns an IWindsorContainer with [ScenarioDependencies]");
            }

            return builder;
        }

        private Func<IWindsorContainer> FindCreateScenarioContainer()
        {
            var assemblies = bindingRegistry.GetBindingAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var methodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                        .FirstOrDefault(mtd => Attribute.IsDefined(mtd, typeof(ScenarioDependenciesAttribute)));

                    if (methodInfo == null)
                    {
                        continue;
                    }

                    var container = methodInfo.Invoke(null, null) as IWindsorContainer;
                    return () => container;
                }
            }

            return null;
        }
    }
}