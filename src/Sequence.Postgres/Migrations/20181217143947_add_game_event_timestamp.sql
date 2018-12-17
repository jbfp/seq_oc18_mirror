ALTER TABLE public.game_event
ADD COLUMN timestamp TIMESTAMPTZ NOT NULL DEFAULT (CURRENT_TIMESTAMP);

INSERT INTO public.migration (name) VALUES ('20181217143947_add_game_event_timestamp');
