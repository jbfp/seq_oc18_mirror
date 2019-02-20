import React, { useState } from 'react';
import { RouteChildrenProps, Redirect } from "react-router";
import { Auth } from '../auth';
import './Login.css';

export default function Login(props: RouteChildrenProps) {
    const [redirectToReferrer, setRedirectToReferrer] = useState(false);
    const [userName, setUserName] = useState('');

    if (redirectToReferrer) {
        const { from } = props.location.state || { from: { pathname: '/' } };

        if (from.pathname === '/login') {
            from.pathname = '/';
        }

        return <Redirect to={from} />
    }

    if (Auth.isAuthenticated) {
        setRedirectToReferrer(true);
    }

    async function submit() {
        await Auth.signInAsync(userName).then(() => setRedirectToReferrer(true));
    }

    return (
        <div>
            <form onSubmit={submit}>
                <p>
                    Please sign in to continue
                </p>

                <div>
                    <input
                        type="text"
                        value={userName}
                        onChange={event => setUserName(event.target.value)}
                        placeholder="Pick a user name"
                        autoFocus={true}
                    />
                </div>

                <div>
                    <button className="login-submit" type="submit" disabled={userName.length === 0}>
                        Sign in
                    </button>
                </div>
            </form>
        </div>
    );
}
