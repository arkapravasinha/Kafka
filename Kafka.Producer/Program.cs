// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;

Console.WriteLine("Kafka Producer");
Console.WriteLine("Press ctrl+c to cancel");
var conf = new ProducerConfig { BootstrapServers = "kafka.confluent.svc.cluster.local:9092" };

Action<DeliveryReport<Null, string>> handler = r =>
    Console.WriteLine(!r.Error.IsError
        ? $"Delivered message to {r.TopicPartitionOffset}"
        : $"Delivery Error: {r.Error.Reason}");
CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

try
{
    using (var p = new ProducerBuilder<Null, string>(conf).Build())
    {
        while (!cts.IsCancellationRequested)
        {
            p.Produce("test_application_topic", new Message<Null, string> { Value = Guid.NewGuid().ToString() }, handler);
        }

        // wait for up to 10 seconds for any inflight messages to be delivered.
        p.Flush(TimeSpan.FromSeconds(10));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception Occured: {ex}");
}
