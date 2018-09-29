using System;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using ProtoBuf;

namespace Lykke.Service.BlockchainHistoryReader.Contract.Events
{
    [ProtoContract]
    public class TransactionCompletedEvent
    {
        /// <summary>
        ///    Amount without fee
        /// </summary>
        [ProtoMember(1)]
        public decimal Amount { get; set; }

        /// <summary>
        ///    Asset ID
        /// </summary>
        [ProtoMember(2)]
        public string AssetId { get; set; }

        /// <summary>
        ///    Blockchain type.
        /// </summary>
        [ProtoMember(3)]
        public string BlockchainType { get; set; }

        /// <summary>
        ///    Source address
        /// </summary>
        [ProtoMember(4)]
        public string FromAddress { get; set; }

        /// <summary>
        ///    Transaction hash as base64 string.
        /// </summary>
        [ProtoMember(5)]
        public string Hash { get; set; }

        /// <summary>
        ///    Transaction moment in UTC
        /// </summary>
        [ProtoMember(6)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///    Destination address
        /// </summary>
        [ProtoMember(7)]
        public string ToAddress { get; set; }

        /// <summary>
        ///    Type of the transaction.
        /// </summary>
        [ProtoMember(8)]
        public TransactionType? TransactionType { get; set; }
        
        /// <summary>
        ///    Client Id.
        /// </summary>
        [ProtoMember(9)]
        public Guid ClientId { get; set; }
    }
}
