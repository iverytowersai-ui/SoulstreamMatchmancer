using System;
using System.Collections.Generic;

namespace Matchmancer.UI
{
    /// <summary>
    /// Pure C# stack-based screen navigation state machine. No Unity —
    /// fully testable. Three operations:
    ///
    ///   • <see cref="Push"/>     — show a new screen on top of the current
    ///     one. The old screen stays on the stack and can be returned to.
    ///   • <see cref="Pop"/>      — go back to the previous screen.
    ///   • <see cref="Replace"/>  — swap the top screen without growing the
    ///     stack (e.g. StageSelect → LevelSelect).
    ///
    /// Events fire AFTER the stack is mutated so listeners always see the
    /// new state.
    ///
    /// The Mono bridge <see cref="ScreenNavigatorController"/> maps these
    /// events to Show/Hide calls on scene GameObjects.
    /// </summary>
    public class ScreenNavigator
    {
        private readonly Stack<ScreenId> _stack = new Stack<ScreenId>();

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>
        /// Fired after the visible screen changes.
        /// (previousScreen, newScreen)
        /// </summary>
        public event Action<ScreenId, ScreenId> OnScreenChanged;

        /// <summary>Fired only on Push — the new screen was added on top.</summary>
        public event Action<ScreenId> OnScreenPushed;

        /// <summary>Fired only on Pop — the top screen was removed.</summary>
        public event Action<ScreenId> OnScreenPopped;

        /// <summary>Fired only on Replace — the top was swapped in place.</summary>
        public event Action<ScreenId, ScreenId> OnScreenReplaced;

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        /// <summary>Currently visible screen (top of stack).</summary>
        public ScreenId Current => _stack.Count > 0 ? _stack.Peek() : ScreenId.None;

        /// <summary>Number of screens on the stack.</summary>
        public int Depth => _stack.Count;

        /// <summary>True if there is at least one screen to go back to.</summary>
        public bool CanPop => _stack.Count > 1;

        /// <summary>True if the stack contains the given screen anywhere.</summary>
        public bool Contains(ScreenId id)
        {
            foreach (var s in _stack)
                if (s == id) return true;
            return false;
        }

        /// <summary>
        /// Return the screen directly below the top, or None if only
        /// one screen is on the stack.
        /// </summary>
        public ScreenId Previous
        {
            get
            {
                if (_stack.Count < 2) return ScreenId.None;
                ScreenId top = _stack.Pop();
                ScreenId prev = _stack.Peek();
                _stack.Push(top);
                return prev;
            }
        }

        // ------------------------------------------------------------------
        // Navigation
        // ------------------------------------------------------------------

        /// <summary>
        /// Push a new screen onto the stack. If the requested screen is
        /// already on top, no-op (prevents double-pushes from fast taps).
        /// </summary>
        public void Push(ScreenId id)
        {
            if (id == ScreenId.None) return;
            ScreenId old = Current;
            if (old == id) return; // already on top

            _stack.Push(id);
            OnScreenPushed?.Invoke(id);
            OnScreenChanged?.Invoke(old, id);
        }

        /// <summary>
        /// Pop the top screen and return to the one below it. No-op if
        /// the stack has only one screen (the root cannot be popped —
        /// use <see cref="Replace"/> or <see cref="Clear"/> instead).
        /// Returns the popped screen, or <see cref="ScreenId.None"/> if
        /// nothing was removed.
        /// </summary>
        public ScreenId Pop()
        {
            if (_stack.Count <= 1) return ScreenId.None;
            ScreenId removed = _stack.Pop();
            ScreenId now = Current;
            OnScreenPopped?.Invoke(removed);
            OnScreenChanged?.Invoke(removed, now);
            return removed;
        }

        /// <summary>
        /// Swap the top screen without changing stack depth. If the stack
        /// is empty, behaves like <see cref="Push"/>. Same-screen replace
        /// is a no-op.
        /// </summary>
        public void Replace(ScreenId id)
        {
            if (id == ScreenId.None) return;
            if (_stack.Count == 0)
            {
                Push(id);
                return;
            }

            ScreenId old = _stack.Peek();
            if (old == id) return;

            _stack.Pop();
            _stack.Push(id);
            OnScreenReplaced?.Invoke(old, id);
            OnScreenChanged?.Invoke(old, id);
        }

        /// <summary>
        /// Clear the entire stack and push a single root screen (the new
        /// "home"). Fires one <see cref="OnScreenChanged"/> from the old
        /// top to the new root. Useful for hard transitions like
        /// "return to hub from any depth".
        /// </summary>
        public void ClearTo(ScreenId root)
        {
            ScreenId old = Current;
            _stack.Clear();
            if (root != ScreenId.None)
                _stack.Push(root);
            ScreenId now = Current;
            if (old != now)
                OnScreenChanged?.Invoke(old, now);
        }

        /// <summary>
        /// Pop everything above a target screen. If the screen is not on
        /// the stack, this is a no-op. Returns how many screens were removed.
        /// </summary>
        public int PopTo(ScreenId target)
        {
            if (!Contains(target)) return 0;
            int removed = 0;
            while (_stack.Count > 0 && _stack.Peek() != target)
            {
                _stack.Pop();
                removed++;
            }
            if (removed > 0)
                OnScreenChanged?.Invoke(ScreenId.None, Current);
            return removed;
        }
    }
}
