using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Queue;
using ProtoBuf;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    [UsedImplicitly]
    public class HistoryUpdateTaskRepository : RepositoryBase, IHistoryUpdateTaskRepository
    {
        private readonly IQueueExt _queue;
        
        
        private HistoryUpdateTaskRepository(
            IQueueExt queue)
        {
            _queue = queue;
        }

        
        public static IHistoryUpdateTaskRepository Create(
            IReloadingManager<string> connectionString)
        {
            Serializer.PrepareSerializer<CompletionToken>();
            Serializer.PrepareSerializer<HistoryUpdateTask>();
            
            var queue = AzureQueueExt.Create
            (
                connectionStringManager: connectionString,
                queueName: "history-update-tasks",
                maxExecutionTimeout: TimeSpan.FromHours(1)
            );
            
            return new HistoryUpdateTaskRepository(queue);
        }
        
        public async Task CompleteAsync(
            string completionToken)
        {
            var (messageId, popReceipt) = DeserializeObject<CompletionToken>(completionToken);

            await _queue.FinishRawMessageAsync(new CloudQueueMessage(messageId, popReceipt));
        }

        public async Task EnqueueAsync(
            HistoryUpdateTask task)
        {
            await _queue.PutRawMessageAsync
            (
                SerializeObject(task)
            );
        }

        public async Task<(HistoryUpdateTask Task, string CompletionToken)> TryGetAsync(
            TimeSpan visibilityTimeout)
        {
            var queueMessage = await _queue.GetRawMessageAsync((int) visibilityTimeout.TotalSeconds);

            if (queueMessage != null)
            {
                var task = DeserializeObject<HistoryUpdateTask>(queueMessage.AsString);

                var token = SerializeObject(new CompletionToken
                {
                    MessageId = queueMessage.Id,
                    PopReceipt = queueMessage.PopReceipt
                });

                return (task, token);
            }
            else
            {
                return (null, null);
            }
        }

        private static string SerializeObject(
            object obj)
        {
            using (var stream = new MemoryStream())  
            {  
                Serializer.Serialize(stream, obj);
                
                return stream
                    .ToArray()
                    .ToHexString();  
            }
        }

        private static T DeserializeObject<T>(
            string str)
        {
            using (var stream = new MemoryStream(str.GetHexStringToBytes()))  
            {  
                return Serializer.Deserialize<T>(stream);  
            }
        }


        [ProtoContract]
        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public class CompletionToken
        {
            [ProtoMember(0)]
            public string MessageId { get; set; }

            [ProtoMember(1)]
            public string PopReceipt { get; set; }

            
            public void Deconstruct(
                out string messageId,
                out string popReceipt)
            {
                messageId = MessageId;
                popReceipt = PopReceipt;
            }
        }
    }
}
