"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./css/main.css");
var signalR = require("@microsoft/signalr");
var divMessages = document.querySelector("#divMessages");
var tbMessage = document.querySelector("#tbMessage");
var btnSend = document.querySelector("#btnSend");
var username = new Date().getTime();
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();
connection.on("showTime", function (time) {
    var timeElement = document.getElementById("currentTime");
    timeElement.innerHTML = time;
});
connection.on("setLight", function (key, status) {
    updateTrafficLight(key, status);
    console.log(key, status);
});
connection.start().catch(function (err) { return document.write(err); });
var updateTrafficLight = function (key, status) {
    var pips = document.querySelectorAll("div#light_".concat(key, " > .pip"));
    pips.forEach(function (p) { return p.classList.remove('on'); });
    var onPip = document.querySelector("div#light_".concat(key, " > .pip.").concat(status));
    onPip.classList.add('on');
};
