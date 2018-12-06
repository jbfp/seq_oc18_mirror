import React from 'react';
import { Route, Redirect } from 'react-router-dom';
import { ServerContext } from "./contexts";
import Layout from './Layout';
import Login from './Login';
import Server from "./server";

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

export const RouteWithLayout = ({ component: Component, render, hash, title, ...rest }) => {
    if (Component) {
        return (
            <Route {...rest} render={props => (
                <Layout hash={hash} title={title} userName={auth.userName} onLogout={auth.signOut}>
                    <Component {...props} />
                </Layout>
            )} />
        );
    }

    return (
        <Route {...rest} render={props => (
            <Layout hash={hash} title={title} userName={auth.userName} onLogout={auth.signOut}>
                {render(props)}
            </Layout>
        )} />
    );
};

export const ProtectedRoute = ({ component: Component, ...rest }) => {
    const RedirectToLogin = ({ location }) => (
        <Redirect to={{
            pathname: "/login",
            state: { from: location }
        }} />
    );

    const ComponentWithContext = props => (
        <ServerContext.Provider value={new Server(window.env.api, auth.userName)}>
            <Component {...props} />
        </ServerContext.Provider>
    );

    const render = (props) => (
        auth.isAuthenticated
            ? ComponentWithContext(props)
            : RedirectToLogin(props)
    );

    return <RouteWithLayout {...rest} render={render} />;
};

export const LoginWithAuth = props => (
    <Login {...props} onLogin={auth.signIn} />
);
