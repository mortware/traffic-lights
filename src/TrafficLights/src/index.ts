import "./css/main.css";
import * as signalR from "@microsoft/signalr";
import {stat} from "fs";

const divMessages: HTMLDivElement = document.querySelector("#divMessages");
const tbMessage: HTMLInputElement = document.querySelector("#tbMessage");
const btnSend: HTMLButtonElement = document.querySelector("#btnSend");
const username = new Date().getTime();

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.on("showTime", (time: string) => {
    let timeElement = document.getElementById("currentTime");
    timeElement.innerHTML = time;
})

connection.on("setLight", (key: string, status: string) => {
    updateTrafficLight(key, status);
    console.log(key, status)
});

connection.start().catch(err => document.write(err));

let updateTrafficLight = (key: string, status: string): void => {
    let pips = document.querySelectorAll(`div#light_${key} > .pip`);
    pips.forEach(p => p.classList.remove('on'));

    let onPip = document.querySelector(`div#light_${key} > .pip.${status}`);
    onPip.classList.add('on');
}