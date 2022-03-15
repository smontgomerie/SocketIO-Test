const mqtt = require('mqtt')
const client  = mqtt.connect('mqtt://test.mosquitto.org')
const {PubSub} = require("graphql-subscriptions");

export const mqtt_pubsub = new PubSub();

client.on('connect', function () {
    let topic1 = 'house2/#';

    client.subscribe(topic1, function (err) {
        if (!err) {
            client.publish('house2/mqtt-test', 'Hello mqtt')
        }
    })
})

client.on('message', function (topic, message) {
    // message is Buffer
    console.log(topic+ ": " + message.toString())
    mqtt_pubsub.publish('MQTT', message)
  //   client.end()
})
