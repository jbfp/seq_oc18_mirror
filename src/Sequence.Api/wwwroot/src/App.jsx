import React from 'react';
import { BrowserRouter as Router, Route } from 'react-router-dom';

import { ServerContext } from './contexts';
import Server from './server';

import Home from './Home';
import Login from './Login';
import Protect from './Protect';

class App extends React.Component {
  state = {
    server: null,
  };

  handleLogin = event => {
    this.setState({ server: new Server(event.userName) });
  };

  handleLogout = () => {
    this.setState({ server: null });
  };

  render() {
    const SeqProtect = ({ component: Component, ...rest }) => (
      <Protect {...rest} component={Component} isAuthenticated={!!this.state.server} loginPath="/login" />
    );

    return (
      <React.StrictMode>
        <Router>
          <div id="app" style={{ 'display': 'flex' }}>
            <Route path="/login" render={props => <Login {...props} onLogin={this.handleLogin} />} />

            <SeqProtect exact path={["/", "/games", "/games/:id"]} component={props =>
              <ServerContext.Provider value={this.state.server}>
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
