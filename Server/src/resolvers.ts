const {PubSub} = require("graphql-subscriptions");

export const pubsub = new PubSub();

export let currentNumber = 0;

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

export function incrementNumber() {
  currentNumber++;
}