"""
Kafka connectivity test module.
"""
import logging
from typing import Optional
from app.services.kafka import KafkaProducerClient, KafkaConsumerClient

logger = logging.getLogger(__name__)

def test_kafka_connectivity(
    bootstrap_servers: str = "kafka:19092",
    test_topic: str = "test_connectivity",
    response_topic: str = "test_response",
    test_message: str = "ping",
    group_id: str = "kafka_connectivity_test_group",
) -> bool:
    """
    Tests Kafka connectivity by producing a message to a test topic and consuming it.
    Sends a response message upon successful consumption.

    Returns:
        bool: True if the test succeeds, False otherwise.
    """
    producer: Optional[KafkaProducerClient] = None
    consumer: Optional[KafkaConsumerClient] = None
    success = False

    try:
        # Initialize clients
        producer = KafkaProducerClient(
            bootstrap_servers=bootstrap_servers,
            value_serializer=lambda v: v.encode("utf-8"),
        )
        consumer = KafkaConsumerClient(
            bootstrap_servers=bootstrap_servers,
            group_id=group_id,
            value_deserializer=lambda v: v.decode("utf-8"),
        )

        # Send test message
        producer.send_message(test_topic, test_message)
        producer.flush()
        logger.info("Produced test message '%s' to topic: %s", test_message, test_topic)

        # Consume message
        consumer.subscribe([test_topic])
        logger.info("Subscribed to topic: %s", test_topic)

        for _ in range(5):  # Attempt 5 times with 2-second timeout
            result = consumer.consume(timeout=2.0)
            if result:
                received_value, msg = result
                logger.info(
                    "Consumed message '%s' from topic %s [partition %s]",
                    received_value,
                    msg.topic(),
                    msg.partition(),
                )

                # Send response
                response_message = f"response_{received_value}"
                producer.send_message(response_topic, response_message)
                producer.flush()
                logger.info("Sent response '%s' to topic: %s", response_message, response_topic)
                success = True
                break

        if not success:
            logger.error("Failed to consume test message after multiple attempts")

    except Exception as e:
        logger.exception("Kafka connectivity test failed: %s", str(e))
        success = False
    finally:
        # Cleanup resources
        if producer:
            producer.flush()
        if consumer:
            consumer.close()
        return success
