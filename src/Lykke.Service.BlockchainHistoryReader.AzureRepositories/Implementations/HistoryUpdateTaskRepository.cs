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
            HistoryUpdateTask task)
        {
            await _queue.FinishRawMessageAsync
            (
                new CloudQueueMessage
                (
                    messageId: task.Id,
                    popReceipt: task.PopReceipt
                )
            );
        }

        public async Task EnqueueAsync(
            HistoryUpdateTask task)
        {
            await _queue.PutRawMessageAsync
            (
                SerializeObject(task)
            );
        }

        public async Task<HistoryUpdateTask> TryGetAsync(
            TimeSpan visibilityTimeout)
        {
            var queueMessage = await _queue.GetRawMessageAsync((int) visibilityTimeout.TotalSeconds);

            if (queueMessage != null)
            {
                var task = DeserializeObject<HistoryUpdateTask>(queueMessage.AsString);

                task.DequeueCount = queueMessage.DequeueCount;
                task.Id = queueMessage.Id;
                task.PopReceipt = queueMessage.PopReceipt;

                return task;
            }
            else
            {
                return null;
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
    }
}
