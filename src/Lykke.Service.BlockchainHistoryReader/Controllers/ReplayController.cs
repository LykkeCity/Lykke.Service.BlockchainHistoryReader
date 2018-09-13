using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.Models;
using Microsoft.AspNetCore.Mvc;


namespace Lykke.Service.BlockchainHistoryReader.Controllers
{
    [PublicAPI, Route("/api/replay")]
    public class ReplayController : Controller
    {
        private readonly IHistoryUpdateService _historyUpdateService;

        public ReplayController(
            IHistoryUpdateService historyUpdateService)
        {
            _historyUpdateService = historyUpdateService;
        }
        
        
        [HttpPost]
        public async Task<IActionResult> ReplayHistory(
            [FromBody] HistorySourceRequest request)
        {
            await _historyUpdateService.ResetLatestHash
            (
                blockchainType: request.BlockchainType,
                address: request.Address
            );

            return Ok();
        }
    }
}