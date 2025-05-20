# Muflone

A CQRS and event store library based on the great work of Jonathan Oliver with [CommonDomain as part of NEventStore](https://github.com/NEventStore/NEventStore)

## Getting started :rocket:

`Install-Package Muflone`

## Sample usage

Look at [this repo](https://github.com/CQRS-Muflone/CQRS-ES_testing_workshop)

### Update 8.3

Since this version we implemented a system to auto-create the consumers so that it is not necessary to create the consumers in your project anymore. This should reduce complexity and potential errors due to a missing consumer.
You can now register directly your command and event handlers and that's it.

Instead of registering the consumers that wrap your handlers

    builder.Services.AddMufloneRabbitMQConsumers(new List<IConsumer>
    {
        new CreateCartConsumer(repository, connectionFactory, loggerFactory),
        new CartCreatedConsumer(repository, connectionFactory, loggerFactory),
        new ProductCreatedConsumer(repository, connectionFactory, loggerFactory),
    }

You can just register them like the following and we will do the rest

    builder.Services.AddCommandHandler<CreateCartHandler>();
    builder.Services.AddDomainEventHandler<CartCreatedHandler>();
    builder.Services.AddIntegrationEventHandler<ProductCreatedHandler>();

In case you have a generic handler that is not a command or event handler, you can still register it like this

    builder.Services.AddGenericHandler<MyGenericHandler>();

We still maintain active the other method to register consumers so that you can extend it if you need to.

## Give a Star! :star:

If you like or are using this project please give it a star. Thanks!
