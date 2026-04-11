using NUnit.Framework;
using Matchmancer.UI;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="ScreenNavigator"/>. No Unity, no scene.
    /// Exercises push/pop/replace/clearTo/popTo + events.
    /// </summary>
    [TestFixture]
    public class ScreenNavigatorTests
    {
        private ScreenNavigator _nav;

        [SetUp]
        public void SetUp()
        {
            _nav = new ScreenNavigator();
        }

        // ==================================================================
        // Initial state
        // ==================================================================

        [Test]
        public void NewNavigator_CurrentIsNone()
        {
            Assert.AreEqual(ScreenId.None, _nav.Current);
            Assert.AreEqual(0, _nav.Depth);
            Assert.IsFalse(_nav.CanPop);
        }

        // ==================================================================
        // Push
        // ==================================================================

        [Test]
        public void Push_SetsCurrentScreen()
        {
            _nav.Push(ScreenId.Loading);
            Assert.AreEqual(ScreenId.Loading, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);
        }

        [Test]
        public void Push_FiresEvents()
        {
            ScreenId pushed = ScreenId.None;
            ScreenId changedFrom = ScreenId.None, changedTo = ScreenId.None;
            _nav.OnScreenPushed  += id => pushed = id;
            _nav.OnScreenChanged += (f, t) => { changedFrom = f; changedTo = t; };

            _nav.Push(ScreenId.MainHub);

            Assert.AreEqual(ScreenId.MainHub, pushed);
            Assert.AreEqual(ScreenId.None,    changedFrom);
            Assert.AreEqual(ScreenId.MainHub, changedTo);
        }

        [Test]
        public void Push_SameScreen_IsNoOp()
        {
            _nav.Push(ScreenId.MainHub);
            int calls = 0;
            _nav.OnScreenChanged += (_, _2) => calls++;
            _nav.Push(ScreenId.MainHub);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(1, _nav.Depth);
        }

        [Test]
        public void Push_None_IsNoOp()
        {
            _nav.Push(ScreenId.None);
            Assert.AreEqual(0, _nav.Depth);
        }

        [Test]
        public void Push_MultipleSetsDepth()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            _nav.Push(ScreenId.LevelSelect);
            Assert.AreEqual(3, _nav.Depth);
            Assert.AreEqual(ScreenId.LevelSelect, _nav.Current);
            Assert.IsTrue(_nav.CanPop);
        }

        // ==================================================================
        // Pop
        // ==================================================================

        [Test]
        public void Pop_ReturnsToPreviousScreen()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            var removed = _nav.Pop();
            Assert.AreEqual(ScreenId.StageSelect, removed);
            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);
        }

        [Test]
        public void Pop_FiresEvents()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);

            ScreenId popped = ScreenId.None;
            ScreenId changedFrom = ScreenId.None, changedTo = ScreenId.None;
            _nav.OnScreenPopped  += id => popped = id;
            _nav.OnScreenChanged += (f, t) => { changedFrom = f; changedTo = t; };

            _nav.Pop();

            Assert.AreEqual(ScreenId.StageSelect, popped);
            Assert.AreEqual(ScreenId.StageSelect, changedFrom);
            Assert.AreEqual(ScreenId.MainHub,     changedTo);
        }

        [Test]
        public void Pop_SingleScreen_IsNoOp()
        {
            _nav.Push(ScreenId.MainHub);
            int calls = 0;
            _nav.OnScreenChanged += (_, _2) => calls++;
            var removed = _nav.Pop();
            Assert.AreEqual(ScreenId.None, removed);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(1, _nav.Depth);
        }

        [Test]
        public void Pop_EmptyStack_IsNoOp()
        {
            var removed = _nav.Pop();
            Assert.AreEqual(ScreenId.None, removed);
        }

        // ==================================================================
        // Replace
        // ==================================================================

        [Test]
        public void Replace_SwapsTopWithoutChangingDepth()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            _nav.Replace(ScreenId.LevelSelect);

            Assert.AreEqual(ScreenId.LevelSelect, _nav.Current);
            Assert.AreEqual(2, _nav.Depth);
        }

        [Test]
        public void Replace_FiresReplacedAndChanged()
        {
            _nav.Push(ScreenId.StageSelect);

            ScreenId repOld = ScreenId.None, repNew = ScreenId.None;
            ScreenId chgFrom = ScreenId.None, chgTo = ScreenId.None;
            _nav.OnScreenReplaced += (o, n) => { repOld = o; repNew = n; };
            _nav.OnScreenChanged  += (f, t) => { chgFrom = f; chgTo = t; };

            _nav.Replace(ScreenId.LevelSelect);

            Assert.AreEqual(ScreenId.StageSelect, repOld);
            Assert.AreEqual(ScreenId.LevelSelect, repNew);
            Assert.AreEqual(ScreenId.StageSelect, chgFrom);
            Assert.AreEqual(ScreenId.LevelSelect, chgTo);
        }

        [Test]
        public void Replace_SameScreen_IsNoOp()
        {
            _nav.Push(ScreenId.MainHub);
            int calls = 0;
            _nav.OnScreenChanged += (_, _2) => calls++;
            _nav.Replace(ScreenId.MainHub);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Replace_EmptyStack_BehavesLikePush()
        {
            _nav.Replace(ScreenId.MainHub);
            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);
        }

        // ==================================================================
        // ClearTo
        // ==================================================================

        [Test]
        public void ClearTo_ResetsStackToSingleRoot()
        {
            _nav.Push(ScreenId.Loading);
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            _nav.Push(ScreenId.LevelSelect);

            _nav.ClearTo(ScreenId.MainHub);

            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);
            Assert.IsFalse(_nav.CanPop);
        }

        [Test]
        public void ClearTo_FiresScreenChanged()
        {
            _nav.Push(ScreenId.Battle);
            ScreenId from = ScreenId.None, to = ScreenId.None;
            _nav.OnScreenChanged += (f, t) => { from = f; to = t; };

            _nav.ClearTo(ScreenId.MainHub);

            Assert.AreEqual(ScreenId.Battle,  from);
            Assert.AreEqual(ScreenId.MainHub, to);
        }

        [Test]
        public void ClearTo_SameAsCurrent_NoEvent()
        {
            _nav.Push(ScreenId.MainHub);
            int calls = 0;
            _nav.OnScreenChanged += (_, _2) => calls++;
            _nav.ClearTo(ScreenId.MainHub);
            Assert.AreEqual(0, calls);
        }

        // ==================================================================
        // PopTo
        // ==================================================================

        [Test]
        public void PopTo_RemovesScreensAboveTarget()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            _nav.Push(ScreenId.LevelSelect);
            _nav.Push(ScreenId.Battle);

            int removed = _nav.PopTo(ScreenId.StageSelect);

            Assert.AreEqual(2, removed);
            Assert.AreEqual(ScreenId.StageSelect, _nav.Current);
            Assert.AreEqual(2, _nav.Depth);
        }

        [Test]
        public void PopTo_TargetNotOnStack_IsNoOp()
        {
            _nav.Push(ScreenId.MainHub);
            int removed = _nav.PopTo(ScreenId.Battle);
            Assert.AreEqual(0, removed);
            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
        }

        [Test]
        public void PopTo_AlreadyOnTop_RemovesZero()
        {
            _nav.Push(ScreenId.MainHub);
            int removed = _nav.PopTo(ScreenId.MainHub);
            Assert.AreEqual(0, removed);
        }

        // ==================================================================
        // Queries
        // ==================================================================

        [Test]
        public void Contains_FindsScreenInStack()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            Assert.IsTrue(_nav.Contains(ScreenId.MainHub));
            Assert.IsTrue(_nav.Contains(ScreenId.StageSelect));
            Assert.IsFalse(_nav.Contains(ScreenId.Battle));
        }

        [Test]
        public void Previous_ReturnsScreenBelowTop()
        {
            _nav.Push(ScreenId.MainHub);
            _nav.Push(ScreenId.StageSelect);
            Assert.AreEqual(ScreenId.MainHub, _nav.Previous);
            Assert.AreEqual(ScreenId.StageSelect, _nav.Current,
                "Previous must not mutate the stack.");
        }

        [Test]
        public void Previous_SingleScreen_ReturnsNone()
        {
            _nav.Push(ScreenId.MainHub);
            Assert.AreEqual(ScreenId.None, _nav.Previous);
        }

        // ==================================================================
        // Full flow: Loading → Login → Hub → Stage → Level → Battle → Results → Hub
        // ==================================================================

        [Test]
        public void FullAppFlow_MatchesExpectedScreenSequence()
        {
            _nav.Push(ScreenId.Loading);
            Assert.AreEqual(ScreenId.Loading, _nav.Current);

            _nav.Replace(ScreenId.Login);
            Assert.AreEqual(ScreenId.Login, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);

            _nav.Replace(ScreenId.MainHub);
            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);

            _nav.Push(ScreenId.StageSelect);
            _nav.Push(ScreenId.LevelSelect);
            _nav.Push(ScreenId.Battle);
            Assert.AreEqual(4, _nav.Depth);

            // Battle ends → push Results
            _nav.Push(ScreenId.Results);
            Assert.AreEqual(ScreenId.Results, _nav.Current);

            // "Return to Hub" button
            _nav.ClearTo(ScreenId.MainHub);
            Assert.AreEqual(ScreenId.MainHub, _nav.Current);
            Assert.AreEqual(1, _nav.Depth);
            Assert.IsFalse(_nav.CanPop);
        }
    }
}
