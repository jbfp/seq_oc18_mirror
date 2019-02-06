export class Status {
    static get UNKNOWN() {
        return new Status(0, '');
    }

    static get OK() {
        return new Status(1, 'Connection: OK');
    }

    static get SERVER_ERROR() {
        return new Status(2, 'Server error');
    }

    static get CONNECTION_ERROR() {
        return new Status(3, 'Connection error');
    }

    constructor(id, name) {
        this._id = id;
        this._name = name;
    }

    valueOf() {
        return this._id;
    }

    toString() {
        return this._name;
    }
}

const HEALTH_URL = `${window.env.api}/health`;

const REQUEST = {
    cache: 'no-store',
    method: 'GET',
};

export async function getHealthAsync() {
    try {
        const response = await window.fetch(HEALTH_URL, REQUEST);

        if (response.ok) {
            return Status.OK;
        } else {
            return Status.SERVER_ERROR;
        }
    } catch {
        return Status.CONNECTION_ERROR;
    }
}
