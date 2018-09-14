using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.Cqrs;

namespace Lykke.Service.BlockchainHistoryReader
{
    public class FixedAutofacDependencyResolver : IDependencyResolver
    {
        private readonly IComponentContext _context;

        /// <summary>
        /// C-tor
        /// </summary>
        /// <param name="context">Autofac component context.</param>
        public FixedAutofacDependencyResolver(
            [NotNull] IComponentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc cref="IDependencyResolver"/>>
        public object GetService(Type type)
        {
            var ctx = _context.Resolve<IComponentContext>();
            
            return ctx.Resolve(type);
        }

        /// <inheritdoc cref="IDependencyResolver"/>>
        public bool HasService(Type type)
        {
            return _context.IsRegistered(type);
        }
    }
}