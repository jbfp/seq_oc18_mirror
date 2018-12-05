import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router, Route, Redirect } from 'react-router-dom';
import { ServerContext } from "./contexts";
import Game from './Game';
import GameList from './GameList';
import Layout from './Layout';
import Login from './Login';
import NewGame from './NewGame';
import Server from "./server";
import './index.css';

const userNameKey = 'user-name';

class AuthService {
    constructor() {
        this._userName = window.localStorage.getItem(userNameKey);
    }

    get isAuthenticated() {
        return !!this.userName;
    }

    get userName() {
        return this._userName;
    }

    signIn = userName => {
        if (!userName) {
            throw new Error('User name is not valid.');
        }

        this._userName = userName;
        window.localStorage.setItem(userNameKey, this.userName);
    }

    signOut = () => {
        this._userName = null;
        window.localStorage.removeItem(userNameKey);
    };
}

const auth = new AuthService();

const RouteWithLayout = ({ component: Component, render, title, ...rest }) => {
    if (Component) {
        return (
            <Route {...rest} render={props => (
                <Layout title={title} userName={auth.userName} onLogout={auth.signOut}>
                    <Component {...props} />
                </Layout>
            )} />
        );
    }

    return (
        <Route {...rest} render={props => (
            <Layout title={title} userName={auth.userName} onLogout={auth.signOut}>
                {render(props)}
            </Layout>
        )} />
    );
};

const ProtectedRoute = ({ component: Component, layout, ...rest }) => {
    const RedirectToLogin = ({ location }) => (
        <Redirect to={{
            pathname: "/login",
            state: { from: location }
        }} />
    );

    const ComponentWithContext = props => (
        <ServerContext.Provider value={new Server(auth.userName)}>
            <Component {...props} />
        </ServerContext.Provider>
    );

    const render = (props) => (
        auth.isAuthenticated
            ? ComponentWithContext(props)
            : RedirectToLogin(props)
    );

    if (layout) {
        return <RouteWithLayout {...rest} render={render} />;
    }

    return <Route {...rest} render={render} />;
};

const LoginWithAuth = props => (
    <Login {...props} onLogin={auth.signIn} />
);

const routing = (
    <Router>
        <div>
            <RouteWithLayout path="/login" layout={true} component={LoginWithAuth} title="sign in" />
            <ProtectedRoute exact path="/" layout={true} component={GameList} title="games" />
            <ProtectedRoute exact path="/new-game" layout={true} component={NewGame} title="new game" />
            <ProtectedRoute path="/games/:id" layout={false} component={Game} title="play" />
        </div>
    </Router>
);

ReactDOM.render(routing, document.getElementById('root'));
