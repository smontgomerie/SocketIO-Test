const {gql} = require("apollo-server-express");
export const {resolvers, incrementNumber, currentNumber, pubsub} = require("./resolvers")

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

