import React, { useEffect, useState } from 'react';
import { Link, Redirect, Route, RouteComponentProps, RouteProps, Switch, withRouter } from 'react-router-dom';
import { Auth } from './auth';
import { ServerContext } from "./contexts";
import { getHashAsync } from './hash';
import Server from "./server";
import { Game } from './Game';
import { Games } from './Games';
import { Heartbeat } from './Heartbeat';
import { Login } from './Login';
import { NewGame } from './NewGame';
import { NewSimulation } from './NewSimulation';
import './App.css';

function ProtectedRoute(props: RouteProps) {
    const { component: Component, ...rest } = props;

    if (!Component) {
        throw new Error('Component is not defined.');
    }

    const render = (props: RouteComponentProps) => (
        Auth.isAuthenticated
            ? <Component {...props} />
            : (<Redirect to={{
                pathname: "/login",
                state: { from: props.location }
            }} />)
    );

    return <Route {...rest} render={render} />;
}

function App(props: RouteProps) {
    const [hash, setHash] = useState('master');

    useEffect(() => {
        getHashAsync().then(setHash);
    }, []);

    const userName = Auth.userName;

    return (
        <div className="layout">
            <div className="layout-header">
                <Link to="/" className="layout-header-link layout-header-title">
                    <h1>
                        one&#8209;eyed&nbsp;jack
                    </h1>
                </Link>

                <Link to={props.location || { pathname: '/' }} className="layout-header-link layout-header-subtitle">
                    <h2>
                        <Switch>
                            <Route path="/login" render={() => 'sign in'} />
                            <Route path="/" exact render={() => 'games'} />
                            <Route path="/new-game" exact render={() => 'new game'} />
                            <Route path="/new-simulation" exact render={() => 'new simulation'} />
                            <Route path="/games/:id" render={() => 'play'} />
                            <Route render={() => 'not found'} />
                        </Switch>
                    </h2>
                </Link>
            </div>

            <hr />

            <div className="layout-body">
                {userName ? (
                    <div>
                        <form className="sign-out-form" onSubmit={() => Auth.signOutAsync()}>
                            <span>Hello, <strong>{userName}</strong></span>
                            <span>   |   </span>
                            <button className="sign-out-btn" type="submit">
                                Sign out
                                </button>
                        </form>
                    </div>
                ) : null}

                <Switch>
                    <Route path="/login" component={Login} />

                    <ServerContext.Provider value={new Server(window.env.api, userName)}>
                        <ProtectedRoute path="/" exact component={Games} />
                        <ProtectedRoute path="/games" exact component={Games} />
                        <ProtectedRoute path="/new-game" exact component={NewGame} />
                        <ProtectedRoute path="/new-simulation" exact component={NewSimulation} />
                        <ProtectedRoute path="/games/:id" component={Game} />
                    </ServerContext.Provider>
                </Switch>
            </div>

            <hr className="layout-body-end" />

            <div className="layout-footer">
                <span>© <a href="/">jbfp.dk</a>&nbsp;<Heartbeat></Heartbeat></span>
                <span>By <a href="mailto:jakob@jbfp.dk">Jakob Pedersen</a></span>
                <a href="https://www.linkedin.com/in/jakob-pedersen-835a824b" target="_blank" rel="noopener noreferrer">LinkedIn</a>
                <a href="https://github.com/jbfp" target="_blank" rel="noopener noreferrer">GitHub</a>
                <a href={`https://github.com/jbfp/one_eyed_jack/commit/${hash}`} target="_blank" rel="noopener noreferrer">{hash}</a>
            </div>
        </div>
    );
}

export default withRouter(App);
