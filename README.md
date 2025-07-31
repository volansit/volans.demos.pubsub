A simple Publish Subscribe messaging demo using .Net Aspire.
Took some inspiration and also code snippets from https://github.com/sa-es-ir/AspireTemplate

PreReqs
---------
- Docker Desktop installed
- .Net 9
- Microsoft Visual Studio 2022

To Run the project
-------
Ctr+F5
You should get the .Net Aspire dashboard. The dashboard will list all the resources that are part of this application including eventbus, RabbitMQConsumer and the PubSub.APIService (the producer of our messages).

To be able to publish a message, use your browser and go to the URL: https://localhost:PORT/send-message. You can get the port nr from Aspire dashboard, eg: 7367.

Above will send a message to the "order" queue.

The RabbitMQConsumer is consumer of the message and so you can observe the messaging coming to the rabbitMQ queue via RabbitMQ's management portal. 

Further one can also go to the RabbitMQConsumer's console log to inspect the incoming messages.
