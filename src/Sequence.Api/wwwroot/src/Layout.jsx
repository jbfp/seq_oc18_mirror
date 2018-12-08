import PropTypes from 'prop-types';
import React from 'react';
import { Redirect } from "react-router-dom";
import './Layout.css';

class Layout extends React.Component {
    static propTypes = {
        hash: PropTypes.string,
        onLogout: PropTypes.func,
        title: PropTypes.string,
        userName: PropTypes.string,
    };

    state = {
        redirectToLogin: false,
    };

    handleLogout = event => {
        event.preventDefault();
        this.props.onLogout();
        this.setState({ redirectToLogin: true });
    }

    render() {
        const { redirectToLogin } = this.state;

        if (redirectToLogin) {
            return <Redirect to="/login" />;
        }

        const { hash, title, userName } = this.props;

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
                    <span>© <a href="/">jbfp.dk</a></span>
                    <span>By <a href="mailto:jakobp10@gmail.com">Jakob Pedersen</a></span>
                    <a href="https://www.linkedin.com/in/jakob-pedersen-835a824b" target="_blank" rel="noopener noreferrer">LinkedIn</a>
                    <a href="https://github.com/jbfp" target="_blank" rel="noopener noreferrer">GitHub</a>
                    {!!hash ? <a href={`https://bitbucket.org/jbfp/sequence_oct18/commits/${hash}`} target="_blank" rel="noopener noreferrer">{hash}</a> : null}
                </div>
            </div>
        )
    }
}

export default Layout;
