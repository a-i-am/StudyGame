using NUnit.Framework;
using StudyGame.Combat;

namespace StudyGame.Combat.Tests
{
    public class DashTimerTests
    {
        DashTimer _timer;

        [SetUp]
        public void SetUp()
        {
            _timer = new DashTimer(0.25f, 0.15f, 0.5f);
        }

        [Test]
        public void StartsInvulnerable()
        {
            Assert.IsFalse(_timer.IsInvulnerable);
            Assert.IsTrue(_timer.TryStart());
            Assert.IsTrue(_timer.IsDashing);
            Assert.IsTrue(_timer.IsInvulnerable);
        }

        [Test]
        public void InvulnerabilityEndsBeforeDash()
        {
            _timer.TryStart();
            _timer.Tick(0.2f);
            Assert.IsTrue(_timer.IsDashing);
            Assert.IsFalse(_timer.IsInvulnerable);
        }

        [Test]
        public void DashEndsAfterDuration()
        {
            _timer.TryStart();
            _timer.Tick(0.25f);
            Assert.IsFalse(_timer.IsDashing);
            Assert.IsFalse(_timer.IsInvulnerable);
        }

        [Test]
        public void CooldownBlocksRestart()
        {
            _timer.TryStart();
            _timer.Tick(0.25f);
            Assert.IsFalse(_timer.TryStart());
            _timer.Tick(0.3f);
            Assert.IsFalse(_timer.CanDash);
            _timer.Tick(0.25f);
            Assert.IsTrue(_timer.TryStart());
            Assert.IsTrue(_timer.IsInvulnerable);
            Assert.AreEqual(0.1f, _timer.Tick(0.1f), 1e-6f);
        }

        [TestCase(1f / 30f)]
        [TestCase(1f / 45f)]
        [TestCase(1f / 165f)]
        [TestCase(0.3f)]
        public void ActiveTimeSumsToDurationAtAnyFrameRate(float dt)
        {
            _timer.TryStart();
            float total = 0f;
            while (_timer.IsDashing) total += _timer.Tick(dt);
            Assert.AreEqual(0.25f, total, 1e-4f);
        }

        [Test]
        public void LongInvulnerabilityEndsWithDash()
        {
            DashTimer timer = new DashTimer(0.25f, 1f, 0f);
            timer.TryStart();
            timer.Tick(0.24f);
            Assert.IsTrue(timer.IsInvulnerable);
            timer.Tick(0.02f);
            Assert.IsFalse(timer.IsDashing);
            Assert.IsFalse(timer.IsInvulnerable);
        }

        [Test]
        public void CannotRestartWhileDashing()
        {
            _timer.TryStart();
            _timer.Tick(0.1f);
            Assert.IsFalse(_timer.TryStart());
        }
    }
}
