# SocketIO-Test
POC for Unity Socket IO

### Usage: 
#### Server
Simple NodeJS server with HTTP backend

`npm install`

`npm start`

#### Unity
- Add TestObject.cs to a GameObject
- Connect to port 11102 (defined in app.js)

#### Web
- Simple web interface to send commands to Unity project
- Most commands wills simply broadcast to unity project
- "set value X" will set the value of the cube's Y value
