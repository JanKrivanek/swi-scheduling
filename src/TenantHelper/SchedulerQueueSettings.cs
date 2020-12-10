namespace TenantHelper
{
    public class SchedulerQueueSettings
    {
        public static SchedulerQueueSettings DefaultSettingsForElasticMQ = new SchedulerQueueSettings()
        {
            QueueServiceUri = "http://localhost:9324",
            QueueServiceAccessKey = "x",
            QueueServiceSecretKey = "x",
            QueueName = "default" //"SampleSchedulingQ"
        };

        /// <summary>
        /// Name of configuration section where this configuration exists
        /// </summary>
        public static readonly string SectionName = "SchedulerQueueSettings";

        /// <summary>
        /// Name of Queue
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Uri of queue service
        /// </summary>
        public string QueueServiceUri { get; set; }

        /// <summary>
        /// Access key for queue service
        /// </summary>
        public string QueueServiceAccessKey { get; set; }

        /// <summary>
        /// Secret key for queue service
        /// </summary>
        public string QueueServiceSecretKey { get; set; }
    }
}