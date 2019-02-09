import React from 'react';
import { ServerContext } from '../contexts';
import { Status, getHealthAsync } from './status';
import './Heartbeat.css';

const DEFAULT_INITIAL_TIMEOUT = 5000;
const DEFAULT_REPEAT_TIMEOUT = 60000;

class Heartbeat extends React.Component {
    static contextType = ServerContext;

    state = {
        status: Status.UNKNOWN,
    };

    _handle = null;

    render() {
        const { status } = this.state;

        return (
            <span
                className="heartbeat"
                data-status={status.valueOf()}
                title={status.toString()}
            ></span>
        );
    }

    componentDidMount() {
        // Start timer initially with shorter delay.
        this._handle = window.setTimeout(this._timerHandler, DEFAULT_INITIAL_TIMEOUT);
    }

    componentWillUnmount() {
        if (this._handle) {
            window.clearInterval(this._handle);
            this._handle = null;
        }
    }

    _timerHandler = async () => {
        this.setState({
            status: await getHealthAsync()
        });

        // Only start timer again if it has been started/not been cancelled.
        if (this._handle) {
            // Start timer again recursively with longer delay.
            this._handle = window.setTimeout(this._timerHandler, DEFAULT_REPEAT_TIMEOUT);
        }
    };
}

export default Heartbeat;
