// @ts-check
const httpServer = require("./httpServer.cjs");

const { createServer } = require("http");
const express = require("express");
const { execute, subscribe } = require("graphql");
const { ApolloServer, gql } = require("apollo-server-express");
const { PubSub } = require("graphql-subscriptions");
const { SubscriptionServer } = require("subscriptions-transport-ws");
const { makeExecutableSchema } = require("@graphql-tools/schema");

(async () => {
    const PORT = 4000;
    const pubsub = new PubSub();
    const app = express();
    const websocketServer = createServer(app);

    // Schema definition
    const typeDefs = gql`
        type Query {
            currentNumber: Int
            hello: String
        }

        type Subscription {
            numberIncremented: Int
            greetings: String
        }
    `;

    // Resolver map
    const resolvers = {
        Query: {
            currentNumber() {
                return currentNumber;
            },
            hello() {
                return "Hello world!";
            }
        },
        Subscription: {
            numberIncremented: {
                subscribe: () => {
                    console.log("subscribe");
                    return pubsub.asyncIterator(["NUMBER_INCREMENTED"])
                },
            },
            greetings : {
                subscribe: () => {
                    console.log("subscribe to greetings");
                    return pubsub.asyncIterator(["GREETINGS"])
                },
            },
        },
    };

    const schema = makeExecutableSchema({ typeDefs, resolvers });

    const server = new ApolloServer({
        schema,
        subscriptions: {
            onConnect: (connectionParams, webSocket, context) => {
                console.log('Connected!')
            },
            onDisconnect: (webSocket, context) => {
                console.log('Disconnected!')
            },
            // ...other options...
        },
    });
    await server.start();
    server.applyMiddleware({ app });

    SubscriptionServer.create(
        { schema, execute, subscribe },
        { server: websocketServer, path: server.graphqlPath }
    );

    websocketServer.listen(PORT, () => {
        console.log(
            `🚀 Query endpoint ready at http://localhost:${PORT}${server.graphqlPath}`
        );
        console.log(
            `🚀 Subscription endpoint ready at ws://localhost:${PORT}${server.graphqlPath}`
        );
    });

    let currentNumber = 0;
    function incrementNumber() {
        currentNumber++;
        pubsub.publish("NUMBER_INCREMENTED", { numberIncremented: currentNumber });
        setTimeout(incrementNumber, 1000);
    }
    function sayHello()
    {
        pubsub.publish("GREETINGS", { greetings: "Hello world!" + currentNumber });
        setTimeout(sayHello, 1000);
    }
    // Start incrementing
    incrementNumber();
    sayHello();
})();