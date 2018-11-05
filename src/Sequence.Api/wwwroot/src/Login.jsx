import PropTypes from 'prop-types';
import React from 'react';
import { Redirect } from 'react-router-dom';

class Login extends React.Component {
  static propTypes = {
    location: PropTypes.object.isRequired,
    onLogin: PropTypes.func.isRequired,
  };

  state = {
    redirectToReferrer: false,
    userName: "",
  };

  handleChange = e => {
    this.setState({ userName: e.target.value });
  };

  handleSubmit = e => {
    e.preventDefault();
    this.props.onLogin({ userName: this.state.userName });
    this.setState({ redirectToReferrer: true });
  };

  render() {
    const { from } = this.props.location.state || { from: { pathname: "/" } };

    if (this.state.redirectToReferrer) {
      return <Redirect to={from} />
    } else {
      return (
        <div id="login">
          <form id="login-form" onSubmit={this.handleSubmit}>
            <label>
              User Name:&nbsp;
              <input
                id="login-form-userName"
                type="text"
                autoFocus={true}
                value={this.state.userName}
                onChange={this.handleChange}
              />
            </label>

            <button id="login-form-submit" type="submit">
              Sign in
          </button>
          </form>
        </div>
      );
    }
  }
}

export default Login;
