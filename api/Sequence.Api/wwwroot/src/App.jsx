import React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import { LoginWithAuth, ProtectedRoute, RouteWithLayout } from './Routing';
import { Game } from './Game';
import GameList from './GameList';
import { NewGame } from './NewGame';

class App extends React.Component {
    state = {
        hash: null,
    };

    async componentDidMount() {
        window
            .fetch('/hash.txt', { headers: { 'Accept': 'text/plain' } })
            .then(response => response.ok ? response.text() : Promise.reject(response.statusText))
            .then(hash => hash.trim().substr(0, 7))
            .then(hash => this.setState({ hash }))
            .catch(() => { /* ignore */ });
    }

    render() {
        const { hash } = this.state;

        return (
            <Router>
                <div>
                    <RouteWithLayout path="/login" component={LoginWithAuth} hash={hash} title="sign in" />
                    <ProtectedRoute exact path="/" component={GameList} hash={hash} title="games" />
                    <ProtectedRoute exact path="/new-game" component={NewGame} hash={hash} title="new game" />
                    <ProtectedRoute path="/games/:id" component={Game} hash={hash} title="play" />
                </div>
            </Router>
        );
    }
}

export default App;
