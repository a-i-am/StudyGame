using System;

namespace StudyGame.Combat
{
    public static class FovMath
    {
        const double Deg2Rad = Math.PI / 180.0;

        public static float VerticalFovForAspect(float baseVerticalFov, float referenceAspect, float aspect)
        {
            if (aspect <= 0f || aspect >= referenceAspect) return baseVerticalFov;
            double halfV = baseVerticalFov * 0.5 * Deg2Rad;
            double tanHalfH = Math.Tan(halfV) * referenceAspect;
            return (float)(2.0 * Math.Atan(tanHalfH / aspect) / Deg2Rad);
        }
    }
}
