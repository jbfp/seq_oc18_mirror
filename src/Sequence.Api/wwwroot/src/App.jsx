import React from 'react';
import { BrowserRouter as Router, Route } from 'react-router-dom';

import { ServerContext } from './contexts';
import Server from './server';

import Home from './Home';
import Login from './Login';
import Protect from './Protect';

const userNameKey = 'user-name';

class App extends React.Component {
  state = {
    server: null,
  };

  constructor(props) {
    super(props);

    const userName = localStorage.getItem(userNameKey);

    if (userName) {
      this.state.server = new Server(userName);
    }
  }

  handleLogin = ({ userName }) => {
    this.setState({ server: new Server(userName) });
    localStorage.setItem(userNameKey, userName);
  };

  handleLogout = () => {
    this.setState({ server: null });
    localStorage.removeItem(userNameKey);
  };

  render() {
    const { server } = this.state;
    const isAuthenticated = server !== null;

    const SeqProtect = ({ component: Component, ...rest }) => (
      <Protect {...rest} component={Component} isAuthenticated={isAuthenticated} loginPath="/login" />
    );

    return (
      <React.StrictMode>
        <Router>
          <div id="app" style={{ 'display': 'flex' }}>
            <Route path="/login" render={props => <Login {...props} onLogin={this.handleLogin} />} />

            <SeqProtect exact path={["/", "/games", "/games/:id"]} component={props =>
              <ServerContext.Provider value={server}>
                <Home {...props} onLogout={this.handleLogout} />
              </ServerContext.Provider>
            } />
          </div>
        </Router>
      </React.StrictMode>
    );
  }
}

export default App;
