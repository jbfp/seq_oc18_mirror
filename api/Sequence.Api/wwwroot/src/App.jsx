import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import { LoginWithAuth, ProtectedRoute, RouteWithLayout } from './Routing';
import { Game } from './Game';
import { Games } from './Games';
import { NewGame } from './NewGame';

const GET_HASH_PATH = '/hash.txt';

const GET_HASH_REQUEST = Object.freeze({
    headers: { 'Accept': 'text/plain' },
});

async function getHashAsync() {
    try {
        const response = await window.fetch(GET_HASH_PATH, GET_HASH_REQUEST);

        if (response.ok) {
            const body = await response.text();
            const hash = body.trim().substr(0, 7);
            return hash;
        }
    } catch {
        // Ignore.
    }

    return 'master';
}

export default function App() {
    const [hash, setHash] = useState('master');

    useEffect(() => {
        getHashAsync().then(setHash);
    });

    return (
        <Router>
            <React.Fragment>
                <RouteWithLayout path="/login" component={LoginWithAuth} hash={hash} title="sign in" />
                <ProtectedRoute exact path="/" component={Games} hash={hash} title="games" />
                <ProtectedRoute exact path="/new-game" component={NewGame} hash={hash} title="new game" />
                <ProtectedRoute path="/games/:id" component={Game} hash={hash} title="play" />
            </React.Fragment>
        </Router>
    );
}
