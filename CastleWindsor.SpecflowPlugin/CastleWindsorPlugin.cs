[assembly: TechTalk.SpecFlow.Plugins.RuntimePlugin(typeof(CastleWindsor.SpecflowPlugin.CastleWindsorPlugin))]

namespace CastleWindsor.SpecflowPlugin
{
    using BoDi;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Infrastructure;
    using TechTalk.SpecFlow.Plugins;
    using TechTalk.SpecFlow.UnitTestProvider;

    public class CastleWindsorPlugin : IRuntimePlugin
    {
        private static object _registrationLock = new object();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            // Wire-up Windsor resolver and container locator to the global IOC container
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;

            // Replace the global IOC container without windsor container
            // which is defined in the method mark with attribute ScenarioDependencies
            runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
        }

        private void RuntimePluginEvents_CustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs args)
        {
            if (args.ObjectContainer.IsRegistered<IContainerFinder>())
            {
                return;
            }

            lock (_registrationLock)
            {
                args.ObjectContainer.RegisterTypeAs<ContainerFinder, IContainerFinder>();
                args.ObjectContainer.RegisterTypeAs<WindsorResolver, ITestObjectResolver>();
            }
        }

        private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs args)
        {
            args.ObjectContainer.RegisterFactoryAs(() =>
            {
                lock (_registrationLock)
                {
                    var containerFinder = args.ObjectContainer.Resolve<IContainerFinder>();
                    var containerBuilder = containerFinder.GetCreateScenarioContainer();
                    var container = containerBuilder.Invoke();

                    RegisterSpecflowDependencies(args.ObjectContainer, container);

                    // Begin the scope to allow the binding file and context to get recreated
                    // before each scenario to ensure fresh dependencies
                    container.BeginScope();

                    return container;
                }
            });
        }

        private void RegisterSpecflowDependencies(IObjectContainer objectContainer, IWindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<IObjectContainer>()
                .UsingFactoryMethod(() => objectContainer)
                .LifestyleTransient()
                .OnlyNewServices());

            windsorContainer.Register(Component.For<ScenarioContext>()
                .UsingFactoryMethod(() => objectContainer.Resolve<ScenarioContext>())
                .LifestyleTransient()
                .OnlyNewServices());

            windsorContainer.Register(Component.For<FeatureContext>()
                .UsingFactoryMethod(() => objectContainer.Resolve<FeatureContext>())
                .LifestyleTransient()
                .OnlyNewServices());

            windsorContainer.Register(Component.For<TestThreadContext>()
                .UsingFactoryMethod(() => objectContainer.Resolve<TestThreadContext>())
                .LifestyleTransient()
                .OnlyNewServices());
        }
    }
}
