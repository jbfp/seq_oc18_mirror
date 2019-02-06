import React from 'react';
import { ServerContext } from '../contexts';
import { Status, getHealthAsync } from './status';
import './Heartbeat.css';

const DEFAULT_INTERVAL = 15000;

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
        this._timerHandler();
        this._handle = window.setInterval(this._timerHandler, DEFAULT_INTERVAL);
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
    };
}

export default Heartbeat;
