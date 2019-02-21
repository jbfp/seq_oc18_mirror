using Sequence.Core;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Sequence.Api
{
    internal sealed class GameViewCollection
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private ImmutableDictionary<PlayerHandle, GameView> _views =
            ImmutableDictionary<PlayerHandle, GameView>.Empty;

        public bool TryGetValue(PlayerHandle key, out GameView view)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _lock.EnterReadLock();

            try
            {
                return _views.TryGetValue(key, out view);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Set(PlayerHandle playerHandle, GameView view)
        {
            if (playerHandle == null)
            {
                throw new ArgumentNullException(nameof(playerHandle));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _lock.EnterWriteLock();

            try
            {
                _views = _views.SetItem(playerHandle, view);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
