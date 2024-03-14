﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Luminance.Common.Easings
{
    public class PiecewiseCurve
    {
        /// <summary>
        /// A piecewise curve that takes up a part of the domain of a <see cref="PiecewiseCurve"/>, specifying the equivalent range and curvature in said domain.
        /// </summary>
        public record CurveSegment(float StartingHeight, float EndingHeight, float AnimationStart, float AnimationEnd, EasingCurves.Curve Curve, EasingType CurveType);

        /// <summary>
        /// The list of <see cref="CurveSegment"/> that encompass the entire 0-1 domain of this function.
        /// </summary>
        protected List<CurveSegment> segments = [];

        public PiecewiseCurve Add(EasingCurves.Curve curve, EasingType curveType, float endingHeight, float animationEnd, float? startingHeight = null)
        {
            float animationStart = segments.Any() ? segments.Last().AnimationEnd : 0f;
            startingHeight ??= segments.Any() ? segments.Last().EndingHeight : 0f;
            if (animationEnd <= 0f || animationEnd > 1f)
                throw new InvalidOperationException("A piecewise animation curve segment cannot have a domain outside of 0-1.");

            // Add the new segment.
            segments.Add(new(startingHeight.Value, endingHeight, animationStart, animationEnd, curve, curveType));

            // Return the piecewise curve that called this method to allow method chaining.
            return this;
        }

        public float Evaluate(float interpolant)
        {
            // Clamp the interpolant into the valid range.
            interpolant = Saturate(interpolant);

            // Calculate the local interpolant relative to the segment that the base interpolant fits into.
            CurveSegment segmentToUse = segments.Find(s => interpolant >= s.AnimationStart && interpolant <= s.AnimationEnd);
            float curveLocalInterpolant = InverseLerp(segmentToUse.AnimationStart, segmentToUse.AnimationEnd, interpolant);

            // Calculate the segment value based on the local interpolant.
            return segmentToUse.Curve.Evaluate(segmentToUse.CurveType, segmentToUse.StartingHeight, segmentToUse.EndingHeight, curveLocalInterpolant);
        }
    }
}
