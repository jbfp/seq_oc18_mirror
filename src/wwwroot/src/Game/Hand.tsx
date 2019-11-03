import React, { useEffect, useMemo, useRef, useState } from 'react';
import * as t from '../types';
import Card from './Card';
import { cardKey } from './helpers';

interface HandProps {
    cards: t.Card[];
    deadCards: Map<string, t.Card>;
    selectedCardKey: string | null;
    onCardClick: (card: t.Card) => void;
}

export default function Hand(props: HandProps) {
    const { cards, deadCards, onCardClick, selectedCardKey } = props;
    const [hideCards, setHideCards] = useState<boolean>(false);
    const hideCardsTimeoutHandle = useRef<number>();
    const touches = useRef<number[]>([]);

    const classes = useMemo(() => {
        const classes = ['hand'];

        if (hideCards) {
            classes.push('hide');
        }

        return classes;
    }, [hideCards]);

    useEffect(() => {
        function handleKeyboardInput(event: KeyboardEvent) {
            const { key } = event;

            // Ignore event if CTRL, SHIFT, etc. is pressed as well.
            if (event.ctrlKey || event.shiftKey || event.altKey || event.metaKey) {
                return;
            }

            const num = Number.parseInt(key, 10);

            if (Number.isSafeInteger(num) && num > 0 && num <= cards.length) {
                event.preventDefault();
                onCardClick(cards[num - 1]);
            }
        }

        window.addEventListener('keydown', handleKeyboardInput, true);

        return () => window.removeEventListener('keydown', handleKeyboardInput, true);
    }, [cards, onCardClick]);

    useEffect(() => {
        function handleTouchStart({ changedTouches }: TouchEvent) {
            const numTouches = changedTouches.length;

            for (let i = 0; i < numTouches; i++) {
                const id = changedTouches[i].identifier;

                if (touches.current.indexOf(id) < 0) {
                    touches.current.push(id);
                }
            }
        }

        window.addEventListener('touchstart', handleTouchStart, false);

        return () => document.removeEventListener('touchstart', handleTouchStart, false);
    }, []);

    useEffect(() => {
        function handleTouchEnd({ changedTouches }: TouchEvent) {
            const numTouches = changedTouches.length;

            for (let i = 0; i < numTouches; i++) {
                const idx = touches.current.indexOf(changedTouches[i].identifier);

                if (idx < 0) {
                    continue;
                }

                touches.current.splice(idx, 1);
            }
        }

        window.addEventListener('touchend', handleTouchEnd, false);

        return () => document.removeEventListener('touchend', handleTouchEnd, false);
    }, []);

    useEffect(() => {
        function handleDeviceOrientation({ beta }: DeviceOrientationEvent) {
            if (beta === null) {
                return;
            }

            const isFlat = Math.abs(beta) <= 18;
            const noTouching = touches.current.length === 0;

            if (isFlat && noTouching) {
                if (hideCardsTimeoutHandle.current) {
                    return;
                }

                hideCardsTimeoutHandle.current = window.setTimeout(() => {
                    setHideCards(true);
                }, 1000);
            } else {
                if (hideCardsTimeoutHandle.current) {
                    window.clearTimeout(hideCardsTimeoutHandle.current);
                    hideCardsTimeoutHandle.current = undefined;
                }

                setHideCards(false);
            }
        }

        window.addEventListener('deviceorientation', handleDeviceOrientation, false);

        return () => window.removeEventListener('deviceorientation', handleDeviceOrientation, false);
    }, [hideCards]);

    const $cards = cards.map((card, idx) => {
        const key = cardKey(card);
        const isDead = deadCards.has(key);

        return (
            <div key={key} className="hand-card">
                <Card
                    card={card}
                    isDead={isDead}
                    isSelected={key === selectedCardKey}
                    onCardClick={onCardClick}
                />

                <kbd>{idx + 1}</kbd>
            </div>
        );
    });

    return (
        <div className={classes.join(' ')}>
            {$cards}
        </div>
    );
}
