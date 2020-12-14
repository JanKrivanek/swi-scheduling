using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SolarWinds.MessageBus.Sqs;
using SolarWinds.Messaging.Abstractions;
using SolarWinds.Messaging.Utils;

namespace TenantHelper
{
    public abstract class BusUserBase : IDisposable
    {
        private readonly SchedulerQueueSettings settings;
        private readonly ILogger logger;

        private IMessageBus messageBus;

        protected BusUserBase(SchedulerQueueSettings settings, ILogger logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        protected IMessageBus MessageBus => messageBus;

        protected string Topic => settings.QueueName;

        protected ILogger Logger => logger;

        protected abstract void OnStart(IMessageBus messageBus);

        public void Start()
        {
            logger.LogInformation("Connecting to queue on {uri}", settings.QueueServiceUri);

            var sqs = new Sqs(
                settings.QueueServiceUri,
                settings.QueueServiceAccessKey,
                settings.QueueServiceSecretKey,
                logger);


            messageBus = new MessageBusBuilder()
                .AddQueueImplementation(sqs)
                .AddTopic(settings.QueueName, ByteToLongParser.Instance)
                .CreateMessageBus();

            OnStart(messageBus);
        }

        public void Dispose()
        {
            messageBus?.Dispose();
        }
    }
}
