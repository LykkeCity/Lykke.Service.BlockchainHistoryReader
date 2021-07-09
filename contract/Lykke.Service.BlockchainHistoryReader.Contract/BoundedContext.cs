using JetBrains.Annotations;

namespace Lykke.Service.BlockchainHistoryReader.Contract
{
    [PublicAPI]
    public class BoundedContext
    {
        public static string Name = "bcn-integration.history-reader";

        public static string EventsRoute = "events";
    }
}
