using System;
using System.Threading.Tasks;
using AzureStorage;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    internal class HistorySourceLockReloadingDecorator : ReloadingOnFailureDecoratorBase<IHistorySourceLockRepository>, IHistorySourceLockRepository
    {
        public HistorySourceLockReloadingDecorator(
            Func<bool, Task<IHistorySourceLockRepository>> makeStorage)
        {
            MakeStorage = makeStorage;
        }
        
        protected override Func<bool, Task<IHistorySourceLockRepository>> MakeStorage { get; }
        
        
        public Task<IHistorySourceLockToken> TryLockAsync()
        {
            return WrapAsync(x => x.TryLockAsync());
        }
    }
}
