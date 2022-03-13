const {PubSub} = require("graphql-subscriptions");
const {gql} = require("apollo-server-express");
export const pubsub = new PubSub();

// Schema definition
export const typeDefs = gql`
    type Query {
        currentNumber: Int
        hello: String
    }

    type Result {
        value: String
        slider: Int
        json: String
    }

    type Subscription {
        numberIncremented: Int
        greetings: Result
    }
`;

// Resolver map
export const resolvers = {
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