using NUnit.Framework;
using StudyGame.Combat;

namespace StudyGame.Combat.Tests
{
    public class FovMathTests
    {
        const float Landscape = 16f / 9f;

        [Test]
        public void LandscapeKeepsBaseFov()
        {
            Assert.AreEqual(50f, FovMath.VerticalFovForAspect(50f, Landscape, Landscape), 1e-4f);
        }

        [Test]
        public void WiderThanReferenceKeepsBaseFov()
        {
            Assert.AreEqual(50f, FovMath.VerticalFovForAspect(50f, Landscape, 21f / 9f), 1e-4f);
        }

        [Test]
        public void PortraitWidensVerticalFovToKeepHorizontal()
        {
            float portrait = FovMath.VerticalFovForAspect(50f, Landscape, 9f / 16f);
            Assert.AreEqual(111.68f, portrait, 0.05f);
        }

        [Test]
        public void InvalidAspectReturnsBase()
        {
            Assert.AreEqual(50f, FovMath.VerticalFovForAspect(50f, Landscape, 0f), 1e-4f);
        }
    }
}
