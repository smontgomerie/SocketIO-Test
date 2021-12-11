// Connects to browser UI

const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const socket = require('socket.io');

var io = socket(server, {
    pingInterval: 10000,
    pingTimeout: 5000,
});

var callbacks = {};

module.exports = {
    foo: function () {
        // whatever
    },
    bar: function () {
        // whatever
    },
    registerCallback: function(id, cb)
    {
        callbacks[id] = cb;
    },
    removeCallback: function(id) {
        delete callbacks[id];
    }

};

var zemba = function () {
}

app.get('/', (req, res) => {
    res.sendFile(__dirname + '/index.html');
});

io.on('connection', (socket) => {
    console.log('a web user connected');

    socket.on('disconnect', () => {
        console.log('user disconnected');
    });

    socket.on('chat message', msg => {
        io.emit('chat message', msg);
        for(var id in callbacks) {
            let cb = callbacks[id];
            cb('chat', msg);
        }
    });
    
    socket.on('slider', msg => {
        for(var id in callbacks) {
            let cb = callbacks[id];
            cb('slider', msg);
        }
    })

    socket.on('json', msg => {
        for(var id in callbacks) {
            let cb = callbacks[id];
            cb('json', msg);
        }
    })

});

server.listen(3000, () => {
    console.log('listening on *:3000');
});
