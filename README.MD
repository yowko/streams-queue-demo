```bash
rabbitmqadmin declare exchange name=test type=topic -u admin -p pass.123 && \
rabbitmqadmin declare queue name=test-streams durable=true queue_type=stream -u admin -p pass.123 && \
rabbitmqadmin declare binding source=test destination_type=queue destination=test-streams routing_key=streams -u admin -p pass.123
```