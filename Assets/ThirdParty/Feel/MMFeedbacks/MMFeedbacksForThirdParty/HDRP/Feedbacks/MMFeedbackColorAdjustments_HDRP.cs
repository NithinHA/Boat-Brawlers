﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// This feedback allows you to control color adjustments' post exposure, hue shift, saturation and contrast over time.
    /// It requires you have in your scene an object with a Volume 
    /// with Color Adjustments active, and a MMColorAdjustmentsShaker_HDRP component.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackPath("PostProcess/Color Adjustments HDRP")]
    [FeedbackHelp("This feedback allows you to control color adjustments' post exposure, hue shift, saturation and contrast over time. " +
            "It requires you have in your scene an object with a Volume " +
            "with Color Adjustments active, and a MMColorAdjustmentsShaker_HDRP component.")]
    public class MMFeedbackColorAdjustments_HDRP : MMFeedback
    {
        /// sets the inspector color for this feedback        
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Color Grading")]
        /// the channel to emit on
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        /// the duration of the shake, in seconds
        [Tooltip("the duration of the shake, in seconds")]
        public float ShakeDuration = 1f;
        /// whether or not to add to the initial intensity
        [Tooltip("whether or not to add to the initial intensity")]
        public bool RelativeIntensity = true;
        /// whether or not to reset shaker values after shake
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        /// whether or not to reset the target's values after shake
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Post Exposure")]
        /// the curve used to animate the focus distance value on
        [Tooltip("the curve used to animate the focus distance value on")]
        public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        /// the value to remap the curve's 0 to
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapPostExposureZero = 0f;
        /// the value to remap the curve's 1 to
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapPostExposureOne = 1f;

        [Header("Hue Shift")]
        /// the curve used to animate the aperture value on
        [Tooltip("the curve used to animate the aperture value on")]
        public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        /// the value to remap the curve's 0 to
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-180f, 180f)]
        public float RemapHueShiftZero = 0f;
        /// the value to remap the curve's 1 to
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-180f, 180f)]
        public float RemapHueShiftOne = 180f;

        [Header("Saturation")]
        /// the curve used to animate the focal length value on
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        /// the value to remap the curve's 0 to
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-100f, 100f)]
        public float RemapSaturationZero = 0f;
        /// the value to remap the curve's 1 to
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-100f, 100f)]
        public float RemapSaturationOne = 100f;

        [Header("Contrast")]
        /// the curve used to animate the focal length value on
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        /// the value to remap the curve's 0 to
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-100f, 100f)]
        public float RemapContrastZero = 0f;
        /// the value to remap the curve's 1 to
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-100f, 100f)]
        public float RemapContrastOne = 100f;
        
        [Header("Color Filter")] 
        /// the selected color filter mode :
        /// None : nothing will happen,
        /// gradient : evaluates the color over time on that gradient, from left to right,
        /// interpolate : lerps from the current color to the destination one 
        [Tooltip("the selected color filter mode :" +
            "None : nothing will happen," +
            "gradient : evaluates the color over time on that gradient, from left to right," +
            "interpolate : lerps from the current color to the destination one ")]
        public MMColorAdjustmentsShaker_HDRP.ColorFilterModes ColorFilterMode = MMColorAdjustmentsShaker_HDRP.ColorFilterModes.None;
        /// the gradient to use to animate the color filter over time
        [Tooltip("the gradient to use to animate the color filter over time")]
        [MMFEnumCondition("ColorFilterMode", (int)MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Gradient)]
        [GradientUsage(true)]
        public Gradient ColorFilterGradient;
        /// the destination color when in interpolate mode
        [Tooltip("the destination color when in interpolate mode")]
        [MMFEnumCondition("ColorFilterMode", (int) MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Interpolate)]
        public Color ColorFilterDestination = Color.yellow;
        /// the curve to use when interpolating towards the destination color
        [Tooltip("the curve to use when interpolating towards the destination color")]
        [MMFEnumCondition("ColorFilterMode", (int) MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Interpolate)]
        public AnimationCurve ColorFilterCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// the duration of this feedback is the duration of the shake
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value; } }

        /// <summary>
        /// Triggers a color adjustments shake
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMColorAdjustmentsShakeEvent_HDRP.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne,
                    ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne,
                    ShakeSaturation, RemapSaturationZero, RemapSaturationOne,
                    ShakeContrast, RemapContrastZero, RemapContrastOne,
                    ColorFilterMode, ColorFilterGradient, ColorFilterDestination, ColorFilterCurve,
                    FeedbackDuration,
                    RelativeIntensity, intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
    }
}
