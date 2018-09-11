using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainWallets.Contract.Events;


namespace Lykke.Service.BlockchainHistoryReader.Workflow
{
    public class WalletOperationsProjection
    {
        private readonly IHistorySourceService _historySourceService;
        

        public WalletOperationsProjection(
            IHistorySourceService historySourceService)
        {
            _historySourceService = historySourceService;
        }


        [UsedImplicitly]
        public Task Handle(
            WalletCreatedEvent evt)
        {
            return _historySourceService.AddHistorySourceIfNotExistsAsync
            (
                blockchainType: evt.BlockchainType,
                address: evt.Address
            );
        }

        [UsedImplicitly]
        public Task Handle(
            WalletDeletedEvent evt)
        {
            return _historySourceService.DeleteHistorySourceIfExistsAsync
            (
                blockchainType: evt.BlockchainType,
                address: evt.Address
            );
        }
    }
}
