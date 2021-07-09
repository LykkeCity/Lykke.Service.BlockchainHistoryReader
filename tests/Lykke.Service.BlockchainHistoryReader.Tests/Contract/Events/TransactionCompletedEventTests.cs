using System;
using System.Buffers.Text;
using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using Lykke.Service.BlockchainHistoryReader.Contract.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace Lykke.Service.BlockchainHistoryReader.Tests.Contract.Events
{
    [TestClass]
    public class TransactionCompletedEventTests
    {
        [TestMethod]
        public void Deserialize__ClientId__Should_Not_BeEmpty()
        {
            var expectedEvent = new TransactionCompletedEvent
            {
                ClientId = Guid.NewGuid()
            };

            byte[] eventBytes;
            
            using (var stream = new MemoryStream())
            {  
                Serializer.Serialize(stream, expectedEvent);

                eventBytes = stream.ToArray();
            }

            TransactionCompletedEvent actualEvent;
            
            using (var stream = new MemoryStream(eventBytes))
            {
                actualEvent = Serializer.Deserialize<TransactionCompletedEvent>(stream);
            }

            actualEvent.ClientId
                .Should().Be(expectedEvent.ClientId);
        }
    }
}