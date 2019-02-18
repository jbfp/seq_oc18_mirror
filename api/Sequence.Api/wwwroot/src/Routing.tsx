import React from 'react';
import { Route, Redirect, RouteProps, RouteComponentProps } from 'react-router-dom';
import { ServerContext } from "./contexts";
import Layout from './Layout';
import { Login } from './Login';
import Server from "./server";

const userNameKey = 'user-name';

class AuthService {
    private _userName: string | null;

    constructor() {
        this._userName = window.localStorage.getItem(userNameKey);
    }

    get isAuthenticated() {
        return !!this.userName;
    }

    get userName() {
        return this._userName;
    }

    signIn = (userName: string) => {
        if (!userName) {
            throw new Error('User name is not valid.');
        }

        this._userName = userName;
        window.localStorage.setItem(userNameKey, userName);
    }

    signOut = () => {
        this._userName = null;
        window.localStorage.removeItem(userNameKey);
    };
}

const auth = new AuthService();

interface RouteWithLayoutProps extends RouteProps {
    hash: string;
    title: string;
}

export const RouteWithLayout = (props: RouteWithLayoutProps) => {
    const { component: Component, hash, render, title, ...rest } = props;

    if (Component) {
        return (
            <Route {...rest} render={props => (
                <Layout hash={hash} title={title} userName={auth.userName} onLogout={auth.signOut}>
                    <Component {...props} />
                </Layout>
            )} />
        );
    } else if (render) {
        return (
            <Route {...rest} render={props => (
                <Layout hash={hash} title={title} userName={auth.userName} onLogout={auth.signOut}>
                    {render(props)}
                </Layout>
            )} />
        );
    }

    return null;
};

export const ProtectedRoute = (props: RouteWithLayoutProps) => {
    const { component: Component, ...rest } = props;

    if (!Component) {
        throw new Error('Component is not defined.');
    }

    const RedirectToLogin = (props: RouteProps) => (
        <Redirect to={{
            pathname: "/login",
            state: { from: props.location }
        }} />
    );

    const ComponentWithContext = (props: any, userName: string) => (
        <ServerContext.Provider value={new Server(window.env.api, userName)}>
            <Component {...props} />
        </ServerContext.Provider>
    );

    const render = (props: RouteProps) => (
        auth.userName
            ? ComponentWithContext(props, auth.userName)
            : RedirectToLogin(props)
    );

    return <RouteWithLayout {...rest} component={render} />;
};

export const LoginWithAuth = (props: any) => (
    <Login {...props} onLogin={auth.signIn} />
);
