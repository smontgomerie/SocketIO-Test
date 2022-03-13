// @ts-check
const httpServer = require("./httpServer.cjs");

const {createServer} = require("http");
const express = require("express");
const {execute, subscribe} = require("graphql");
const {ApolloServer, gql} = require("apollo-server-express");
const {PubSub} = require("graphql-subscriptions");
const {SubscriptionServer} = require("subscriptions-transport-ws");
const {makeExecutableSchema} = require("@graphql-tools/schema");
const {User, WebUser} = require("./user.js");

(async () => {
    const PORT = 4000;
    const pubsub = new PubSub();
    const app = express();
    const websocketServer = createServer(app);
    const users = {};

    // Schema definition
    const typeDefs = gql`
        type Query {
            currentNumber: Int
            hello: String
        }
        
        type Result {
            value: Int
            slider: Int
            json: String
        }

        type Subscription {
            numberIncremented: Int
            greetings: Result
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
            greetings: {
                subscribe: () => {
                    console.log("subscribe to greetings");
                    return pubsub.asyncIterator(["GREETINGS"])
                },
            },
        },
    };

    const schema = makeExecutableSchema({typeDefs, resolvers});

    const server = new ApolloServer({schema});
    await server.start();
    server.applyMiddleware({app});

    SubscriptionServer.create(
        {
            schema, execute, subscribe,
            onConnect: (connectionParams, webSocket, context) => {
                console.log('Connected! ' + webSocket.id);
                const socket = {
                    id: 0
                }

                httpServer.registerCallback(socket.id, (type, message) => {
                    const user = {
                        id: socket.id
                    };

                    users[socket.id] ||= user;

                    if (type == 'chat') {
                        console.log("socket.emit web message " + message)
                        if (message.startsWith("set value")) {
                            socket.emit("set value", message.substr("set value ".length));
                        } else {
                            socket.emit("web message", message);
                            socket.emit("chat", message);
                        }
                    } else if (type == 'slider') {
                        console.log("socket.emit slider message " + message)
                        pubsub.publish("GREETINGS", {greetings: {value: 0, slider: message, json: null}});
                    } else if (type == 'json') {
                        console.log("socket.emit json message " + message)
                        // socket.emit("json", message);
                        pubsub.publish("GREETINGS", {greetings: {value: 0, slider: 0, json: message}});


                        // Cache it
                        users[socket.id].data = message;
                    }
                });
            },
            onOperation: (message, params, webSocket) => {
                // Manipulate and return the params, e.g.
                // params.context.randomId = uuid.v4();

                // Or specify a schema override
                // if (shouldOverrideSchema()) {
                //     params.schema = newSchema;
                // }

                return params;
            },
            onOperationComplete: webSocket => {
                // ...
            },
            onDisconnect: (webSocket, context) => {
                // ...
                console.log('Disconnected!'  + webSocket.id)
            },
        },
        {server: websocketServer, path: server.graphqlPath}
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
        pubsub.publish("NUMBER_INCREMENTED", {numberIncremented: currentNumber});
        setTimeout(incrementNumber, 1000);
    }

    function sayHello() {
        pubsub.publish("GREETINGS", {greetings: {value: 0, slider: 0, json: JSON.stringify({message: "Hello world!"})}});
        setTimeout(sayHello, 1000);
    }

    // Start incrementing
    incrementNumber();
   // sayHello();
})();