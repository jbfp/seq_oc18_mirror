// EventTarget shim for Safari.
import { EventTarget } from 'event-target-shim';
import * as SignalR from '@aspnet/signalr';

const URL = `${window.env.api}/myHub`;
const CONNECTION_OPTIONS = Object.freeze({
    skipNegotiation: true,
    transport: SignalR.HttpTransportType.WebSockets,
});

const API_EVENT = 'UpdateGame';
const MY_EVENT = 'game-event';

class GameEvent extends CustomEvent {
    constructor(data) {
        super(MY_EVENT, { detail: data });
    }
}

async function startAsync(connection) {
    try {
        await connection.start();
    } catch (err) {
        setTimeout(() => startAsync(connection), 5000);
    }
}

export default async function subscribeAsync(gameId) {
    const connection = new SignalR.HubConnectionBuilder()
        .withUrl(URL, { ...CONNECTION_OPTIONS })
        .configureLogging(SignalR.LogLevel.Information)
        .build();

    connection.onclose(() => startAsync(this));

    await connection.start();
    await connection.invoke('Subscribe', gameId);

    return new MyClient(connection);
}

export class MyClient {
    _target = new EventTarget();

    constructor(connection) {
        connection.on(API_EVENT, gameEvent => {
            this._target.dispatchEvent(new GameEvent(gameEvent));
        });
    }

    addGameEventListener(listener) {
        this._target.addEventListener(MY_EVENT, listener, false);
    }

    removeGameEventListener(listener) {
        this._target.removeEventListener(MY_EVENT, listener, false);
    }
}
