declare global {
    interface Document {
        mozHidden: string | undefined;
        msHidden: string | undefined;
        webkitHidden: string | undefined;
    }
}

interface PageVisibility {
    hidden: string;
    visibilityChange: string;
}

let keys: PageVisibility | null;

if (typeof document.hidden !== "undefined") {
    keys = { hidden: 'hidden', visibilityChange: 'visibilitychange' };
} else if (typeof document.mozHidden !== "undefined") {
    keys = { hidden: 'mozHidden', visibilityChange: 'mozvisibilitychange' };
} else if (typeof document.msHidden !== "undefined") {
    keys = { hidden: 'msHidden', visibilityChange: 'msvisibilitychange' };
} else if (typeof document.webkitHidden !== "undefined") {
    keys = { hidden: 'webkitHidden', visibilityChange: 'webkitvisibilitychange' };
} else {
    keys = null;
}

export default Object.freeze(keys);
