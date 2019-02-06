// EventTarget shim for Safari.
import { EventTarget } from 'event-target-shim';

const HOST = window.env.api;
const SSE_EVENT = 'game-updated';
const API_EVENT = 'game-event';

class GameEvent extends CustomEvent {
    constructor(data) {
        super(API_EVENT, { detail: data });
    }
}

export default class GameEventSource {
    _target = new EventTarget();
    _numListeners = 0;
    _sse = null;
    _url = null;

    constructor(gameId, playerId) {
        if (!gameId) {
            throw new Error(`Game ID '${gameId}' is not valid.`);
        }

        if (!playerId) {
            throw new Error(`Player ID '${playerId}' is not valid.`);
        }

        this._url = `${HOST}/games/${gameId}/stream?player=${playerId}`;
    }

    addGameEventListener(listener) {
        this._target.addEventListener(API_EVENT, listener, false);
        this._numListeners++;

        if (this._numListeners === 1) {
            this._sse = new EventSource(this._url);
            this._sse.addEventListener(SSE_EVENT, this._onGameUpdated, false);
        }
    }

    removeGameEventListener(listener) {
        this._target.removeEventListener(API_EVENT, listener, false);
        this._numListeners--;

        if (this._numListeners === 0) {
            this._sse.removeEventListener(SSE_EVENT, this._onGameUpdated, false);
            this._sse.close();
            this._sse = null;
        }
    }

    _onGameUpdated = e => {
        const init = JSON.parse(e.data);
        const event = new GameEvent(init);
        this._target.dispatchEvent(event);
    };
}
