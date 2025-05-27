"""
Kafka service module providing producer and consumer clients for interacting with Apache Kafka.
"""
import json
import logging
import threading
from confluent_kafka import Producer, Consumer, KafkaError, KafkaException

class KafkaProducerClient:
    """
    A client for producing messages to Kafka topics.

    Attributes:
        producer (Producer): Underlying confluent-kafka Producer instance.
        logger (logging.Logger): Logger instance for producer operations.
    """

    def __init__(
        self, bootstrap_servers,
        value_serializer=lambda v: v, key_serializer=lambda k: k, **configs):
        """
        Initialize Kafka producer client.

        Args:
            bootstrap_servers (str): Comma-separated list of broker addresses.
            value_serializer (callable): Function to serialize message values.
            key_serializer (callable): Function to serialize message keys.
            configs (dict): Additional producer configuration parameters.
        """
        self.logger = logging.getLogger(self.__class__.__name__)
        self.value_serializer = value_serializer
        self.key_serializer = key_serializer

        conf = {
            'bootstrap.servers': bootstrap_servers,
            'message.send.max.retries': 3,
            'retry.backoff.ms': 10000
        }
        conf.update(configs)
        self.producer = Producer(conf)
        self.logger.info("Initialized producer for servers: %s", bootstrap_servers)

    def send_message(self, topic, value, key=None):
        """
        Produce a message to the specified Kafka topic.

        Args:
            topic (str): Target topic name.
            value (any): Message value to be serialized and sent.
            key (any): Optional message key for partitioning.

        Raises:
            KafkaException: If message production fails.
        """
        def delivery_callback(err, msg):
            if err:
                self.logger.error("Message delivery failed: %s", err)
            else:
                self.logger.debug("Message delivered to %s [%s]", msg.topic(), msg.partition())

        try:
            serialized_value = self.value_serializer(value)
            serialized_key = self.key_serializer(key) if key is not None else None

            self.producer.produce(
                topic=topic,
                value=serialized_value,
                key=serialized_key,
                callback=delivery_callback
            )
            self.producer.poll(0)
            self.logger.debug("Message enqueued for topic: %s", topic)
        except (KafkaException, BufferError) as e:
            self.logger.error("Failed to produce message to topic %s: %s", topic, e)
            raise

    def flush(self):
        """Flush outstanding messages and wait for completion."""
        try:
            remaining = self.producer.flush()
            self.logger.info("Flushed producer messages. Remaining: %s", remaining)
        except KafkaException as e:
            self.logger.error("Error flushing producer: %s", e)
            raise

class KafkaConsumerClient:
    """
    A client for consuming messages from Kafka topics.

    Attributes:
        consumer (Consumer): Underlying confluent-kafka Consumer instance.
        logger (logging.Logger): Logger instance for consumer operations.
    """

    def __init__(self, bootstrap_servers, group_id, value_deserializer=lambda v: v, **configs):
        """
        Initialize Kafka consumer client.

        Args:
            bootstrap_servers (str): Comma-separated list of broker addresses.
            group_id (str): Consumer group ID for offset management.
            value_deserializer (callable): Function to deserialize message values.
            configs (dict): Additional consumer configuration parameters.
        """
        self.logger = logging.getLogger(self.__class__.__name__)
        self.value_deserializer = value_deserializer

        conf = {
            'bootstrap.servers': bootstrap_servers,
            'group.id': group_id,
            'auto.offset.reset': 'earliest',
            'enable.auto.commit': False,
            'message.send.max.retries': 10,
            'retry.backoff.ms': 10000,
            'max.poll.interval.ms': 60000,
        }
        conf.update(configs)
        self.consumer = Consumer(conf)
        self.logger.info("Initialized consumer for group: %s", group_id)

    def subscribe(self, topics):
        """
        Subscribe to list of topics.

        Args:
            topics (list): List of topic names to subscribe to.

        Raises:
            KafkaException: If subscription fails.
        """
        try:
            self.consumer.subscribe(topics)
            self.logger.info("Subscribed to topics: %s", topics)
        except KafkaException as e:
            self.logger.error("Subscription failed: %s", e)
            raise

    def consume(self, timeout=1.0):
        """
        Poll for messages from subscribed topics.

        Args:
            timeout (float): Polling timeout in seconds.

        Returns:
            tuple: (deserialized message value, message metadata) or None if timeout.

        Raises:
            KafkaException: For critical consumer errors.
        """
        try:
            msg = self.consumer.poll(timeout)
            if msg is None:
                return None

            if msg.error():
                if msg.error().code() == KafkaError.ERR__PARTITION_EOF:
                    self.logger.debug("Reached end of partition %s", msg.partition())
                    return None
                raise KafkaException(msg.error())

            value = self.value_deserializer(msg.value())
            self.logger.debug("Received message from %s [%s]", msg.topic(), msg.partition())
            return value, msg

        except KafkaException as e:
            self.logger.error("Consume error: %s", e)
            raise

    def commit(self, message=None):
        """
        Commit message offset(s) to Kafka.

        Args:
            message (Message): Specific message to commit. If None, commits all.

        Raises:
            KafkaException: If commit fails.
        """
        try:
            if message:
                self.consumer.commit(message, asynchronous=False)
            else:
                self.consumer.commit(asynchronous=False)
            self.logger.debug("Committed offsets")
        except KafkaException as e:
            self.logger.error("Commit failed: %s", e)
            raise

    def close(self):
        """Close consumer connection."""
        try:
            self.consumer.close()
            self.logger.info("Consumer closed")
        except KafkaException as e:
            self.logger.error("Error closing consumer: %s", e)
            raise

class ThreadedKafkaConsumer(threading.Thread):
    """
    A threaded Kafka consumer that runs in a separate thread and processes messages via callbacks.

    Attributes:
        consumer (Consumer): Underlying confluent-kafka Consumer instance
        logger (logging.Logger): Logger instance for consumer operations
        shutdown_flag (threading.Event): Flag for graceful shutdown
    """
    def __init__(self, bootstrap_servers, group_id, topics,
                 value_deserializer=lambda v: v,
                 message_callback=None,
                 error_callback=None,
                 **configs):
        """
        Initialize threaded Kafka consumer.

        Args:
            bootstrap_servers (str): Comma-separated list of broker addresses
            group_id (str): Consumer group ID for offset management
            topics (list): List of topics to subscribe to
            value_deserializer (callable): Message value deserializer
            message_callback (function): Callback for processed messages (value, metadata)
            error_callback (function): Callback for critical errors
            configs (dict): Additional consumer configuration
        """
        super().__init__()
        self.logger = logging.getLogger(self.__class__.__name__)
        self.value_deserializer = value_deserializer
        self.message_callback = message_callback
        self.error_callback = error_callback
        self.shutdown_flag = threading.Event()

        conf = {
            'bootstrap.servers': bootstrap_servers,
            'group.id': group_id,
            'auto.offset.reset': 'earliest',
            'enable.auto.commit': False,
            'max.poll.interval.ms': 60000,
            'message.send.max.retries': 3,
            'retry.backoff.ms': 10000
        }
        conf.update(configs)

        self.logger.debug("Initializing with config: %s", conf)

        self.consumer = Consumer(conf)
        self.topics = topics
        self.logger.info("Initialized threaded consumer for group: %s", group_id)

    def run(self):
        """Main consumer thread loop."""
        try:
            self.consumer.subscribe(self.topics)
            self.logger.info("Consumer thread started for topics: %s", self.topics)

            while not self.shutdown_flag.is_set():
                msg = self.consumer.poll(1.0)

                if msg is None:
                    continue

                if msg.error():

                    if msg.error().code() == KafkaError._PARTITION_EOF:
                        self.logger.debug("Reached end of partition %s", msg.partition())
                        continue
                    if self.error_callback:
                        self.error_callback(msg.error())
                    continue

                try:
                    value = self.value_deserializer(msg.value())
                    self.logger.debug("Received message from %s [%s]", msg.topic(), msg.partition())

                    if self.message_callback:

                        decoded_payload = json.loads(value.decode('utf-8'))
                        self.logger.debug("Passing value %s to the message_callback method...", decoded_payload)
                        self.message_callback(decoded_payload)

                    self.consumer.commit(msg)
                except KafkaException as e:
                    self.logger.exception("Kafka error processing message: %s", e)
                    if self.error_callback:
                        self.error_callback(e)
                except (ValueError, TypeError) as e:
                    self.logger.exception("Data deserialization failed: %s", e)
                    if self.error_callback:
                        self.error_callback(e)
                except Exception as e:  # pylint: disable=W0718
                    self.logger.exception("Unexpected processing error: %s", e)
                    if self.error_callback:
                        self.error_callback(e)

        except KafkaException as e:
            self.logger.error("Consumer thread error: %s", e)
            if self.error_callback:
                self.error_callback(e)
        finally:
            self.close()
            self.logger.info("Consumer thread stopped")

    def stop(self):
        """Trigger graceful shutdown of the consumer thread."""
        self.shutdown_flag.set()
        self.logger.info("Shutdown signal sent to consumer thread")

    def close(self):
        """Cleanup consumer resources."""
        try:
            self.consumer.close()
            self.logger.debug("Consumer resources cleaned up")
        except KafkaException as e:
            self.logger.error("Error closing consumer: %s", e)
