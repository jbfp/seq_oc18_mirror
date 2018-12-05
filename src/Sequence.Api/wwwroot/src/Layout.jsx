import PropTypes from 'prop-types';
import React from 'react';
import { Redirect } from "react-router-dom";
import './Layout.css';

class Layout extends React.Component {
    static propTypes = {
        onLogout: PropTypes.func,
        title: PropTypes.string,
        userName: PropTypes.string,
    };

    state = {
        hash: null,
        redirectToLogin: false,
    };

    handleLogout = event => {
        event.preventDefault();
        this.props.onLogout();
        this.setState({ redirectToLogin: true });
    }

    async componentDidMount() {
        window
            .fetch('/hash.txt', { headers: { 'Accept': 'text/plain' } })
            .then(response => response.ok ? response.text() : Promise.reject())
            .then(hash => hash.trim().substr(0, 7))
            .then(hash => this.setState({ hash }));
    }

    render() {
        const { hash, redirectToLogin } = this.state;

        if (redirectToLogin) {
            return <Redirect to="/login" />;
        }

        const { title, userName } = this.props;

        const $signOut = !!userName
            ? (
                <div>
                    <form className="sign-out-form" onSubmit={this.handleLogout}>
                        <span>Hello, <strong>{userName}</strong></span>
                        <span>   |   </span>
                        <button className="sign-out-btn" type="submit">
                            Sign out
                        </button>
                    </form>
                </div>
            ) : null;

        return (
            <div className="layout">
                <div className="layout-header">
                    <h1 className="layout-header-title">
                        Sequence
                    </h1>

                    <h2 className="layout-header-subtitle">
                        {title}
                    </h2>
                </div>

                <hr />

                <div className="layout-body">
                    {$signOut}

                    {this.props.children}
                </div>

                <hr className="layout-body-end" />

                <div className="layout-footer">
                    <span>© <a href="/">jbfp.dk</a></span>&nbsp;
                    <span>By <a href="mailto:jakobp10@gmail.com">Jakob Pedersen</a></span>&nbsp;
                    <a href="https://www.linkedin.com/in/jakob-pedersen-835a824b" target="_blank" rel="noopener noreferrer">LinkedIn</a>
                    <a href="https://github.com/jbfp" target="_blank" rel="noopener noreferrer">GitHub</a>
                    {!!hash ? <a href={`https://bitbucket.org/jbfp/sequence_oct18/commits/${hash}`} target="_blank" rel="noopener noreferrer">{hash}</a> : null}
                </div>
            </div>
        )
    }
}

export default Layout;
