import { UDPWebSocket } from "./UDPWebSocket.js";

let _ws;
let _ip;
let _port;  

console.log("TEEEEEEEEEEES")
export function initializeUDP(ip, port) {
        _ip = ip;
        _port = port;
        console.log(`Initializing UDP WebSocket on port ${_port} with ip ${_ip}`);
        _ws = new UDPWebSocket(`ws://${_ip}:${_port}`);
        console.log(`Initialized UDP WebSocket on port ${_port} with ip ${_ip}`);
        _ws.onmessage = ev =>{
            console.log('Received UDP message:', ev.data);
        }
    
};