import React from 'react';
import { Redirect } from "react-router-dom";
import './Login.css';

class Login extends React.Component {
    state = {
        redirectToReferrer: false,
        userName: '',
    };

    handleSubmit = event => {
        event.preventDefault();

        this.props.onLogin(this.state.userName);

        this.setState({
            redirectToReferrer: true,
            userName: ''
        });
    };

    handleUserNameChange = event => {
        event.preventDefault();
        this.setState({ userName: event.target.value });
    };

    render() {
        const { redirectToReferrer, userName } = this.state;

        if (redirectToReferrer) {
            const { from } = this.props.location.state || { from: { pathname: '/' } };
            return <Redirect to={from} />;
        }

        const disabled = userName.length === 0;

        return (
            <div>
                <form onSubmit={this.handleSubmit}>
                    <p>
                        Please sign in to continue
                    </p>

                    <div>
                        <input
                            type="text"
                            value={userName}
                            onChange={this.handleUserNameChange}
                            placeholder="Pick a user name"
                        />
                    </div>

                    <div>
                        <button className="login-submit" type="submit" disabled={disabled}>
                            Sign in
                        </button>
                    </div>
                </form>
            </div>
        );
    }
}

export default Login;
