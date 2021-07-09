using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainHistoryReader.Controllers
{
    [PublicAPI, Route("/api/history-sources")]
    public class HistorySourcesController : Controller
    {
        private readonly IHistorySourceService _historySourceService;


        public HistorySourcesController(
            IHistorySourceService historySourceService)
        {
            _historySourceService = historySourceService;
        }
        

        [HttpPost]
        public async Task<IActionResult> AddHistorySource(
            [FromBody] HistorySourceRequest request)
        {
            await _historySourceService.AddHistorySourceIfNotExistsAsync
            (
                blockchainType: request.BlockchainType,
                address: request.Address,
                clientId: request.ClientId
            );
            
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteHistorySource(
            [FromBody] HistorySourceRequest request)
        {
            await _historySourceService.DeleteHistorySourceIfExistsAsync
            (
                blockchainType: request.BlockchainType,
                address: request.Address
            );

            return Ok();
        }
    }
}