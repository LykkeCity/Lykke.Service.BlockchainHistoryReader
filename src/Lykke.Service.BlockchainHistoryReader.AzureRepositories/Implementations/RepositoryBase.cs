using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    public abstract class RepositoryBase
    {
        private static readonly object InitLock = new object();
        
        private static bool _initialized;
        
        
        
        protected RepositoryBase()
        {
            Initialize();
        }

        private static void Initialize()
        {
            lock (InitLock)
            {
                if (!_initialized)
                {
                    var provider = new CompositeMetamodelProvider()
                        .AddProvider
                        (
                            new AnnotationsBasedMetamodelProvider()
                        );

                    EntityMetamodel.Configure(provider);

                    _initialized = true;
                }
            }
        }
    }
}
