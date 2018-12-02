class Server {
  _userName = null;

  constructor(userName) {
    if (!userName) {
      throw new Error(`User name '${userName}' is not valid.`);
    }

    this._userName = userName;
  }

  get userName() {
    return this._userName;
  }

  async createGameAsync(opponent) {
    if (!opponent) {
      throw new Error(`'${opponent}' is not valid.`);
    }

    const response = await fetch('/api/games', {
      body: JSON.stringify({
        'opponent': opponent,
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

  async getGamesAsync() {
    const response = await fetch('/api/games', {
      headers: {
        'Accept': 'application/json',
        'Authorization': this._userName,
      },
      method: 'GET',
    });

    const body = await response.json();

    return body['gameIds'];
  }

  async getGameByIdAsync(id) {
    if (!id) {
      throw new Error(`Game ID '${id}' is not valid.`);
    }

    const response = await fetch(`/api/games/${id}`, {
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

    const response = await fetch(`/api/games/${id}`, {
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
