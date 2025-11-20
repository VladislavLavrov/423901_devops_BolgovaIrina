using _1_Calculator.Services;
using Confluent.Kafka;
using Microsoft.VisualBasic;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
namespace _1_Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        IProducer<K, V> kafkaHandle;
        public KafkaProducerService(KafkaProducerHandler handle)
        {
        kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
        }
         public Task ProduceAsync(string topic, Message<K,
        V> message)
         => kafkaHandle.ProduceAsync(topic, message);
         public void Produce(string topic, Message<K,
        V> message, Action<DeliveryReport<K, V>> deliveryHandler =
        null)
         => kafkaHandle.Produce(topic, message,
        deliveryHandler);
        public void Flush(TimeSpan timeout)
        => kafkaHandle.Flush(timeout);
    }
}