CREATE EXTENSION pgcrypto;

CREATE TABLE public.migration
( id SERIAL NOT NULL
, name TEXT NOT NULL
, timestamp timestamptz NOT NULL DEFAULT (CURRENT_TIMESTAMP)
, PRIMARY KEY (id)
);

CREATE UNIQUE INDEX uq_migration_name
ON public.migration (name);

CREATE TABLE public.game
( id SERIAL NOT NULL
, game_id UUID NOT NULL DEFAULT (gen_random_uuid())
, player1 TEXT NOT NULL
, player2 TEXT NOT NULL
, seed INTEGER NOT NULL
, version SMALLINT NOT NULL
, PRIMARY KEY (id)
);

CREATE UNIQUE INDEX uq_game_game_id
ON public.game (game_id)
INCLUDE (player1, player2, seed, version);

CREATE INDEX ix_game_player1
ON public.game (player1)
INCLUDE (game_id);

CREATE INDEX ix_game_player2
ON public.game (player2)
INCLUDE (game_id);

CREATE TYPE public.deckno AS ENUM ('one', 'two');
CREATE TYPE public.suit AS ENUM ('hearts', 'spades', 'diamonds', 'clubs');
CREATE TYPE public.rank AS ENUM ('ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king');

CREATE TYPE public.card AS
( deckno deckno
, suit suit
, rank rank
);

CREATE TYPE public.chip AS ENUM ('red', 'green', 'blue');

CREATE TYPE public.coord AS
( col SMALLINT
, row SMALLINT
);

CREATE TABLE public.game_event
( game_id INTEGER NOT NULL
, idx INTEGER NOT NULL
, by_player_id TEXT NOT NULL
, card_drawn public.card NULL
, card_used public.card NOT NULL
, chip public.chip NULL
, coord public.coord NOT NULL
, next_player_id TEXT NULL
, PRIMARY KEY (game_id)
, UNIQUE (game_id, idx)
);

INSERT INTO public.migration (name) VALUES ('20181215121402_initial');
