import React, { useState } from 'react';
import { Redirect } from "react-router-dom";
import { Heartbeat } from './Heartbeat';
import './Layout.css';

interface LayoutProps {
    children: React.ReactNode | React.ReactNode[];
    hash: string;
    title: string;
    userName: string | null;
    onLogout: () => void;
}

export default function Layout(props: LayoutProps) {
    const [redirectToLogin, setRedirectToLogin] = useState(false);

    function logout() {
        props.onLogout();
        setRedirectToLogin(true);
    }

    if (redirectToLogin) {
        return <Redirect to="/login" />;
    }

    return (
        <div className="layout">
            <div className="layout-header">
                <h1 className="layout-header-title">
                    one&#8209;eyed&nbsp;jack
                </h1>

                <h2 className="layout-header-subtitle">
                    {props.title}
                </h2>
            </div>

            <hr />

            <div className="layout-body">
                {props.userName ? (
                    <div>
                        <form className="sign-out-form" onSubmit={logout}>
                            <span>Hello, <strong>{props.userName}</strong></span>
                            <span>   |   </span>
                            <button className="sign-out-btn" type="submit">
                                Sign out
                            </button>
                        </form>
                    </div>
                ) : null}
                {props.children}
            </div>

            <hr className="layout-body-end" />

            <div className="layout-footer">
                <span>© <a href="/">jbfp.dk</a>&nbsp;<Heartbeat></Heartbeat></span>
                <span>By <a href="mailto:jakob@jbfp.dk">Jakob Pedersen</a></span>
                <a href="https://www.linkedin.com/in/jakob-pedersen-835a824b" target="_blank" rel="noopener noreferrer">LinkedIn</a>
                <a href="https://github.com/jbfp" target="_blank" rel="noopener noreferrer">GitHub</a>
                <a href={`https://github.com/jbfp/one_eyed_jack/commit/${props.hash}`} target="_blank" rel="noopener noreferrer">{props.hash}</a>
            </div>
        </div>
    );
}
