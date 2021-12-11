'use strict';

const httpServer = require("./httpServer");

const http = require('http');
const socket = require('socket.io');
const server = http.createServer();
const port = 11002;
const prefix = "V2: ";
const nspPrefix = "/nsp," + prefix;

var io = socket(server, {
    pingInterval: 2500,
    pingTimeout: 5000,
});

io.use((socket, next) => {
    console.log("unity user connected. token:" + socket.handshake.query.token + " id = " + socket.id);
    if (socket.handshake.query.token === "V2") {
        next();
    } else {
        console.log("Authentication error");
        next(new Error("Authentication error"));
    }
})

io.on('connection', socket => {

    console.log("hooking up onMessage")
    httpServer.registerCallback(socket.id, (type, message) => {
        if (type == 'chat') {
            console.log("socket.emit web message " + message)
            if (message.startsWith("set value")) {
                socket.emit("set value", message.substr("set value ".length));
            } else {
                socket.emit("web message", message);
                socket.emit("chat", message);
            }
        }
        else if ( type == 'slider')
        {
            console.log("socket.emit slider message " + message)
            socket.emit("set value", message);
        }
        else if ( type == 'json')
        {
            console.log("socket.emit json message " + message)
            socket.emit("json", message);
        }
    });

    socket.on('hi', (msg) => {
        console.log("hi " + msg)
        socket.emit('hi', prefix + msg);
    });

    socket.on('headers', cb => {
        cb(socket.request.headers)
    });

    socket.on('welcome', () => {
        console.log("welcome " + socket.id)
        socket.emit('welcome', Buffer.from("welcome " + socket.id, 'utf8'), Buffer.from("test", 'utf8'));
    });

    socket.on("no params", () => {
        socket.emit("no params");
    });

    socket.on("1 params", p1 => {
        socket.emit("1 params", p1);
    });

    socket.on("2 params", (p1, p2) => {
        socket.emit("2 params", p1, p2);
    });

    socket.on("3 params", (p1, p2, p3) => {
        socket.emit("3 params", p1, p2, p3);
    });

    socket.on("4 params", (p1, p2, p3, p4) => {
        socket.emit("4 params", p1, p2, p3, p4);
    });

    socket.on("5 params", (p1, p2, p3, p4, p5) => {
        socket.emit("5 params", p1, p2, p3, p4, p5);
    });

    socket.on("bytes-0", data => {
        const reqStr = data.toString();
        const bytes = Buffer.from(prefix + reqStr, "utf-8");
        socket.emit("bytes-0", bytes);
    });

    socket.on("bytes-1", data => {
        const text = data.text.toString();
        const bytes = Buffer.from(prefix + text, "utf-8");
        socket.emit("bytes-1", {
            name: prefix + data.name,
            text: bytes
        });
    });


    // server calls the client's callback
    socket.on("no params | cb: no params", cb => {
        cb();
    });

    socket.on("1 params | cb: 1 params", (p1, cb) => {
        cb(p1);
    });

    socket.on("2 params | cb: 2 params", (p1, p2, cb) => {
        cb(p1, p2);
    });

    socket.on("3 params | cb: 3 params", (p1, p2, p3, cb) => {
        cb(p1, p2, p3);
    });

    socket.on("4 params | cb: 4 params", (p1, p2, p3, p4, cb) => {
        cb(p1, p2, p3, p4);
    });

    socket.on("5 params | cb: 5 params", (p1, p2, p3, p4, p5, cb) => {
        cb(p1, p2, p3, p4, p5);
    });

    socket.on("client calls the server's callback 0", () => {
        socket.emit("client calls the server's callback 0", () => {
            socket.emit("no params");
        });
    });

    socket.on("client calls the server's callback 1", p1 => {
        socket.emit("client calls the server's callback 1", p1, (arg1) => {
            socket.emit("1 params", arg1);
        });
    });

    socket.on("client calls the server's callback 2", (p1, p2) => {
        socket.emit("client calls the server's callback 2", p1, p2, (arg1, arg2) => {
            socket.emit("2 params", arg1, arg2);
        });
    });

    socket.on("client calls the server's callback 3", (p1, p2, p3) => {
        socket.emit("client calls the server's callback 3", p1, p2, p3, (arg1, arg2, arg3) => {
            socket.emit("3 params", arg1, arg2, arg3);
        });
    });

    socket.on("client calls the server's callback 4", (p1, p2, p3, p4) => {
        socket.emit("client calls the server's callback 4", p1, p2, p3, p4, (arg1, arg2, arg3, arg4) => {
            socket.emit("4 params", arg1, arg2, arg3, arg4);
        });
    });

    socket.on("client calls the server's callback 5", (p1, p2, p3, p4, p5) => {
        socket.emit("client calls the server's callback 5", p1, p2, p3, p4, p5, (arg1, arg2, arg3, arg4, arg5) => {
            socket.emit("5 params", arg1, arg2, arg3, arg4, arg5);
        });
    });

    socket.on("sever disconnect", close => {
        httpServer.removeCallback(close.id);

        socket.disconnect(close)
    });
});

const nsp = io.of("/nsp");
nsp.use((socket, next) => {
    console.log("nsp")
    if (socket.handshake.query.token === "V2") {
        console.log("nsp token:" + socket.handshake.query.token);
        next();
    } else {
        console.log("Authentication error nsp");
        next(new Error("Authentication error nsp"));
    }
})
nsp.on("connection", socket => {
    socket.on('hi', (msg) => {
        socket.emit('hi', nspPrefix + msg);
    });

    socket.on("no params", () => {
        socket.emit("no params");
    });

    socket.on("1 params", p1 => {
        socket.emit("1 params", p1);
    });

    socket.on("2 params", (p1, p2) => {
        socket.emit("2 params", p1, p2);
    });

    socket.on("3 params", (p1, p2, p3) => {
        socket.emit("3 params", p1, p2, p3);
    });

    socket.on("4 params", (p1, p2, p3, p4) => {
        socket.emit("4 params", p1, p2, p3, p4);
    });

    socket.on("5 params", (p1, p2, p3, p4, p5) => {
        socket.emit("5 params", p1, p2, p3, p4, p5);
    });

    socket.on("bytes-0", data => {
        const reqStr = data.toString();
        const bytes = Buffer.from(nspPrefix + reqStr, "utf-8");
        socket.emit("bytes-0", bytes);
    });

    socket.on("bytes-1", data => {
        const text = data.text.toString();
        const bytes = Buffer.from(nspPrefix + text, "utf-8");
        socket.emit("bytes-1", {
            name: nspPrefix + data.name,
            text: bytes
        });
    });


    // server calls the client's callback
    socket.on("no params | cb: no params", cb => {
        cb();
    });

    socket.on("1 params | cb: 1 params", (p1, cb) => {
        cb(p1);
    });

    socket.on("2 params | cb: 2 params", (p1, p2, cb) => {
        cb(p1, p2);
    });

    socket.on("3 params | cb: 3 params", (p1, p2, p3, cb) => {
        cb(p1, p2, p3);
    });

    socket.on("4 params | cb: 4 params", (p1, p2, p3, p4, cb) => {
        cb(p1, p2, p3, p4);
    });

    socket.on("5 params | cb: 5 params", (p1, p2, p3, p4, p5, cb) => {
        cb(p1, p2, p3, p4, p5);
    });

    socket.on("client calls the server's callback 0", () => {
        socket.emit("client calls the server's callback 0", () => {
            socket.emit("no params");
        });
    });

    socket.on("client calls the server's callback 1", p1 => {
        socket.emit("client calls the server's callback 1", p1, (arg1) => {
            socket.emit("1 params", arg1);
        });
    });

    socket.on("client calls the server's callback 2", (p1, p2) => {
        socket.emit("client calls the server's callback 2", p1, p2, (arg1, arg2) => {
            socket.emit("2 params", arg1, arg2);
        });
    });

    socket.on("client calls the server's callback 3", (p1, p2, p3) => {
        socket.emit("client calls the server's callback 3", p1, p2, p3, (arg1, arg2, arg3) => {
            socket.emit("3 params", arg1, arg2, arg3);
        });
    });

    socket.on("client calls the server's callback 4", (p1, p2, p3, p4) => {
        socket.emit("client calls the server's callback 4", p1, p2, p3, p4, (arg1, arg2, arg3, arg4) => {
            socket.emit("4 params", arg1, arg2, arg3, arg4);
        });
    });

    socket.on("client calls the server's callback 5", (p1, p2, p3, p4, p5) => {
        socket.emit("client calls the server's callback 5", p1, p2, p3, p4, p5, (arg1, arg2, arg3, arg4, arg5) => {
            socket.emit("5 params", arg1, arg2, arg3, arg4, arg5);
        });
    });

    socket.on("sever disconnect", close => {
        socket.disconnect(close)
    });
});

server.listen(port, () => {
    console.log('listening on *:' + port);
});