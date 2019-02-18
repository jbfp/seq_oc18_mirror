import { History, Location } from "history";
import React, { useState } from 'react';
import './Login.css';

interface LoginProps {
    history: History;
    location: Location
    onLogin: (userName: string) => void;
}

export default function Login(props: LoginProps) {
    const [userName, setUserName] = useState('');

    function submit() {
        props.onLogin(userName);

        const { from } = props.location.state || { from: { pathname: '/' } };

        if (from.pathname === '/login') {
            from.pathname = '/';
        }

        props.history.push(from);
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
