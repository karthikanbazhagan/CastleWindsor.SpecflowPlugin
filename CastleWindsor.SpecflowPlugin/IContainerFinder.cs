namespace CastleWindsor.SpecflowPlugin
{
    using System;
    using Castle.Windsor;

    internal interface IContainerFinder
    {
        Func<IWindsorContainer> GetCreateScenarioContainer();
    }
}