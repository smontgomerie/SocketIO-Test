// @ts-check
import {incrementNumber, currentNumber, pubsub, resolvers, typeDefs} from "./src/schema";

const httpServer = require("./httpServer.cjs");

const {createServer} = require("http");
const express = require("express");
const {execute, subscribe} = require("graphql");
const {ApolloServer} = require("apollo-server-express");
const {SubscriptionServer} = require("subscriptions-transport-ws");
const {makeExecutableSchema} = require("@graphql-tools/schema");

import {findUser, validateToken} from "./src/auth";

const {Result} = require("./src/generated/graphql");

(async () => {
  const PORT = 4000;
  const app = express();
  const websocketServer = createServer(app);

  const schema = makeExecutableSchema({typeDefs, resolvers});

  const server = new ApolloServer({schema});
  await server.start();
  server.applyMiddleware({app});

  SubscriptionServer.create(
    {
      schema, execute, subscribe,
      onConnect: (connectionParams, webSocket, context) => {
        let authToken = connectionParams?.authToken || context?.request?.headers?.authorization;
        console.log('Connected! ' + 'authToken: ' + authToken);

        if (authToken) {
          authToken = authToken.replace('Bearer ', '');
        }

        if (authToken) {
          return validateToken(authToken)
            .then(findUser(authToken))
            .then((user) => {

              httpServer.registerCallback(authToken, (type, message) => {
                if (type == 'chat') {
                  console.log("socket.emit web message " + message)
                  if (message.startsWith("set value")) {
                    // socket.emit("set value", message.substr("set value ".length));
                    const result: typeof Result = {
                      value: message.substr("set value ".length),
                      slider: null,
                      json: null
                    };
                    pubsub.publish("GREETINGS", {
                      greetings: result
                    });
                  } else {
                    // socket.emit("web message", message);
                    // socket.emit("chat", message);
                    pubsub.publish("GREETINGS", {greetings: {value: "", slider: message, json: null}});
                  }
                } else if (type == 'slider') {
                  console.log("socket.emit slider message " + message)
                  let greetings: typeof Result = {
                    value: "",
                    slider: message,
                    json: null
                  };
                  pubsub.publish("GREETINGS", {
                    greetings: greetings
                  });
                } else if (type == 'json') {
                  console.log("socket.emit json message " + message)
                  // socket.emit("json", message);
                  let result: typeof Result = {
                    value: "",
                    slider: 0,
                    json: message
                  };
                  pubsub.publish("GREETINGS", {
                    greetings: result
                  });

                  // Cache it
                  user.data = message;
                }
                return {
                  currentUser: user,
                };
              });

              // Return previously cached data
              if ( user.data != null ) {
                let result: typeof Result = {
                  value: "",
                  slider: 0,
                  json: user.data
                };

                pubsub.publish("GREETINGS", {
                  greetings: result
                });
              }
            });
        }

        throw new Error('Missing auth token!');
      },
      onOperation: (message, params, webSocket) => {
        // Manipulate and return the params, e.g.
        // params.context.randomId = uuid.v4();

        // Or specify a schema override
        // if (shouldOverrideSchema()) {
        //     params.schema = newSchema;
        // }

        console.log('onOperation' + message + ' params: ' + params + ' webSocket: ' + webSocket);

        return params;
      },
      onOperationComplete: webSocket => {
        console.log('onOperationComplete' + webSocket);
      },
      onDisconnect: (webSocket, context) => {
        // ...
        console.log('Disconnected!' + webSocket.id)
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

  function incrementNumberL() {
    incrementNumber();
    pubsub.publish("NUMBER_INCREMENTED", {numberIncremented: currentNumber});
    setTimeout(incrementNumber, 1000);
  }

  function sayHello() {
    pubsub.publish("GREETINGS", {greetings: {value: "", slider: 0, json: JSON.stringify({message: "Hello world!"})}});
    setTimeout(sayHello, 1000);
  }

  // Start incrementing
  incrementNumberL();
  // sayHello();
})();