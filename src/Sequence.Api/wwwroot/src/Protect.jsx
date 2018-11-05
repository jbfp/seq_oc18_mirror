import PropTypes from 'prop-types';
import React from 'react';
import { Route, Redirect } from 'react-router-dom';

class Protect extends React.Component {
  static propTypes = {
    component: PropTypes.oneOfType([
      PropTypes.object,
      PropTypes.func,
    ]).isRequired,
    isAuthenticated: PropTypes.bool.isRequired,
    loginPath: PropTypes.string.isRequired,
  };

  render() {
    const { component: Component, isAuthenticated, loginPath, ...rest } = this.props;

    const renderChild = props => {
      if (isAuthenticated) {
        return <Component {...props} />;
      } else {
        const redirect = {
          pathname: loginPath,
          state: { from: props.location }
        };

        return <Redirect to={redirect} />;
      }
    };

    return <Route {...rest} render={renderChild} />;
  }
}

export default Protect;
