import "./css/main.css";
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.on("updateTime", (time: string, schedule: string) => {
    let timeElement = document.getElementById("currentTime");
    timeElement.innerHTML = time + ` (${schedule})`;
})

connection.on("updateTrafficLight", (key: string, status: string) => {
    updateTrafficLight(key, status);
});

connection.on("updateCurrentFlowInfo", (currentFlowName: string, nextFlowName: string) => {
    let currentFlowNameElement = document.getElementById("currentFlowName");
    currentFlowNameElement.innerHTML = `Current: ${currentFlowName}`;

    let nextFlowNameElement = document.getElementById("nextFlowName");
    nextFlowNameElement.innerHTML = `Next: ${nextFlowName}`;
});

connection.start().catch(err => document.write(err));

let updateTrafficLight = (key: string, status: string): void => {
    let pips = document.querySelectorAll(`div#light_${key} > .pip`);
    pips.forEach(p => p.classList.remove('on'));

    let pipToActivate = document.querySelector(`div#light_${key} > .pip.${status}`);
    pipToActivate.classList.add('on');
}

// Debug
let updateClock = () => {
    let now = new Date();
    document.getElementById('debugLocalTime').innerHTML = now.toTimeString();
    setTimeout(updateClock,100);
}
updateClock();