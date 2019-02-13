class Server {
  _endpoint = null;
  _userName = null;

  constructor(endpoint, userName) {
    if (!endpoint) {
      throw new Error(`Endpoint '${endpoint}' is not valid.`);
    }

    if (!userName) {
      throw new Error(`User name '${userName}' is not valid.`);
    }

    this._endpoint = endpoint;
    this._userName = userName;
  }

  get userName() {
    return this._userName;
  }

  async createGameAsync(opponents, boardType, numSequencesToWin) {
    if (!opponents) {
      throw new Error(`'${opponents}' is not valid.`);
    }

    if (typeof boardType !== 'number') {
      throw new Error(`'${boardType}' is not valid.`);
    }

    if (typeof numSequencesToWin !== 'number') {
      throw new Error(`'${numSequencesToWin}' is not valid.`);
    }

    const response = await fetch(`${this._endpoint}/games`, {
      body: JSON.stringify({
        'boardType': boardType,
        'numSequencesToWin': numSequencesToWin,
        'opponents': opponents,
      }),
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    const body = await response.json();

    if (response.ok) {
      return body['gameId'];
    } else {
      throw new Error(body.error);
    }
  }

  async getBoardAsync(boardType) {
    if (!boardType) {
      throw new Error(`'${boardType}' is not a valid board type.`);
    }

    const response = await fetch(`${this._endpoint}/boards/${boardType}`, {
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
      },
      method: 'GET',
    });

    return await response.json();
  }

  async getBotsAsync() {
    const response = await fetch(`${this._endpoint}/bots`, {
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
      },
      method: 'GET',
    });

    const body = await response.json();

    return body['botTypes'];
  }

  async getGamesAsync() {
    const response = await fetch(`${this._endpoint}/games`, {
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
      },
      method: 'GET',
    });

    const body = await response.json();

    return body['games'];
  }

  async getGameByIdAsync(id) {
    if (!id) {
      throw new Error(`Game ID '${id}' is not valid.`);
    }

    const response = await fetch(`${this._endpoint}/games/${id}`, {
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
      },
      method: 'GET',
    });

    const body = await response.json();

    return body['game'];
  }

  async playCardAsync(id, card, coord) {
    if (!id) {
      throw new Error(`Game ID '${id}' is not valid.`);
    }

    if (!card) {
      throw new Error(`Card '${card}' is not valid.`);
    }

    if (!coord) {
      throw new Error(`Coord '${coord}' is not valid.`);
    }

    const response = await fetch(`${this._endpoint}/games/${id}`, {
      body: JSON.stringify({
        card, ...coord
      }),
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
        'Content-Type': 'application/json'
      },
      method: 'POST',
    });

    const body = await response.json();

    return body;
  }
}

export default Server;
