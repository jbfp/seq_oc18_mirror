import React, { useCallback, useContext, useMemo, useRef, useEffect, useState } from 'react';
import * as SignalR from '@aspnet/signalr';
import { ServerContext } from "../contexts";
import { GameState, reducer } from './reducer';
import * as t from "../types";
import GameView from './GameView';

interface GameProps {
    id: t.GameId;
    init: GameState;
    onRequestReload: () => Promise<void>;
}


const HUB_URL = `${window.env.api}/game-hub`;

const CONNECTION_OPTIONS: SignalR.IHttpConnectionOptions = {
    skipNegotiation: true,
    transport: SignalR.HttpTransportType.WebSockets,
    logMessageContent: true,
};

async function startAsync(connection: SignalR.HubConnection, callback: () => Promise<void>) {
    console.log('Trying to connect...');

    try {
        await connection.start();

        try {
            await callback();
        } catch (err) {
            console.error('Error executing SignalR connection callback', err);
        }
    } catch (err) {
        setTimeout(async () => await startAsync(connection, callback), 5000);
    }
}

export default function Game(props: GameProps) {
    const { init } = props;
    const gameId = props.id;
    const playerId = init.playerId;
    const [state, setState] = useState<GameState>(init);
    const [selectedCard, setSelectedCard] = useState<t.Card | null>(null);
    const connection = useRef<SignalR.HubConnection>();
    const showNotifications = useRef<boolean | null>(null);
    const server = useContext(ServerContext);

    const handleUpdatesAsync = useCallback((update: t.GameUpdated) => {
        setState(state => {
            if (state.version + 1 === update.version) {
                return reducer(state, update);
            } else {
                console.log(update, state.version);
                props.onRequestReload();
                return state;
            }
        });
    }, [props.onRequestReload]);

    useEffect(() => {
        const conn = new SignalR.HubConnectionBuilder()
            .withUrl(HUB_URL, { ...CONNECTION_OPTIONS })
            .configureLogging(SignalR.LogLevel.Information)
            .build();

        conn.on('UpdateGame', msg => handleUpdatesAsync(msg));

        async function handleConnectionStarted() {
            await conn.invoke('Identify', playerId);
        }

        conn.onclose(async error => {
            if (error) {
                await startAsync(conn, handleConnectionStarted);
            } else {
                // Connection closed gracefully.
            }
        });

        startAsync(conn, handleConnectionStarted);

        connection.current = conn;

        return () => {
            conn.stop();
        };
    }, [gameId, handleUpdatesAsync, playerId]);

    useEffect(() => {
        if (showNotifications.current === null && typeof Notification !== 'undefined') {
            const handlePermissionCallback = (result: NotificationPermission) => {
                showNotifications.current = result === 'granted'
            };

            const promise = Notification.requestPermission();

            if (typeof promise === 'undefined') {
                // Safari on MacOS only supports the old requestPermission function.
                Notification.requestPermission(handlePermissionCallback);
            } else {
                promise.then(handlePermissionCallback);
            }
        }
    }, []);

    const handleCardClick = useCallback((card: t.Card) => {
        setSelectedCard(selectedCard => {
            if (selectedCard === card) {
                return null;
            } else {
                return card;
            }
        });
    }, []);

    const handleCoordClickAsync = useCallback(async (coord: t.Coord) => {
        if (selectedCard) {
            try {
                const result = await server.playCardAsync(gameId, selectedCard, coord);
                console.assert(result.updates.length === 1);
                setSelectedCard(null);
                await handleUpdatesAsync(result.updates[0]);
            } catch (err) {
                alert(err.toString());
            }
        }
    }, [gameId, handleUpdatesAsync, selectedCard]);

    const handleExchangeDeadCardClick = useCallback(async () => {
        if (selectedCard) {
            try {
                const result = await server.exchangeDeadCardAsync(gameId, selectedCard);
                console.assert(result.updates.length === 1);
                setSelectedCard(null);
                await handleUpdatesAsync(result.updates[0]);
            } catch (err) {
                alert(err.toString());
            }
        }
    }, [gameId, handleUpdatesAsync, selectedCard]);

    return (
        <GameView
            game={state}
            selectedCard={selectedCard}
            onCardClick={handleCardClick}
            onCoordClick={handleCoordClickAsync}
            onExchangeDeadCardClick={handleExchangeDeadCardClick}
        />
    );
}
