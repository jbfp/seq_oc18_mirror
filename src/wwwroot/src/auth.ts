const USER_NAME_KEY = 'user-name';

class Authentication {
    private _userName = window.localStorage.getItem(USER_NAME_KEY) || '';

    get userName() {
        return this._userName;
    }

    get isAuthenticated() {
        return this.userName.length > 0;
    }

    async signInAsync(userName: string) {
        if (userName.length === 0) {
            throw new Error('user name is not valid.');
        }

        this._userName = userName;
        window.localStorage.setItem(USER_NAME_KEY, this._userName);
    }

    async signOutAsync() {
        this._userName = '';
        window.localStorage.removeItem(USER_NAME_KEY);
    }
}

export const Auth = new Authentication();
