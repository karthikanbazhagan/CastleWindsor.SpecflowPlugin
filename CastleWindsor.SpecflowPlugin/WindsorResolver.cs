namespace CastleWindsor.SpecflowPlugin
{
    using System;
    using BoDi;
    using Castle.Windsor;
    using TechTalk.SpecFlow.Infrastructure;

    internal class WindsorResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            var windsorContainer = container.Resolve<IWindsorContainer>();
            return windsorContainer.Resolve(bindingType);
        }
    }
}