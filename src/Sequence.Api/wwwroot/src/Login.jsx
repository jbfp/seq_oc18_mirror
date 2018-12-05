import React from 'react';
import './Login.css';

class Login extends React.Component {
    state = {
        userName: '',
    };

    handleSubmit = event => {
        event.preventDefault();
        const { from } = this.props.location.state || { from: { pathname: '/' } };
        this.props.onLogin(this.state.userName);
        this.setState({ userName: '' });
        this.props.history.push(from);
    };

    handleUserNameChange = event => {
        event.preventDefault();
        this.setState({ userName: event.target.value });
    };

    render() {
        const { userName } = this.state;
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
