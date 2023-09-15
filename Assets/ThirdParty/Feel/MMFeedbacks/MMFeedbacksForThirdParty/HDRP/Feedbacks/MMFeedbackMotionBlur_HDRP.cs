﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// This feedback allows you to control HDRP motion blur intensity over time.
    /// It requires you have in your scene an object with a Volume 
    /// with MotionBlur active, and a MMMotionBlurShaker_HDRP component.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackPath("PostProcess/Motion Blur HDRP")]
    [FeedbackHelp("This feedback allows you to control motion blur intensity over time. " +
            "It requires you have in your scene an object with a Volume " +
            "with MotionBlur active, and a MMMotionBlurShaker_HDRP component.")]
    public class MMFeedbackMotionBlur_HDRP : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Motion Blur")]
        /// the channel to emit on
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        /// the duration of the shake, in seconds
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 0.2f;
        /// whether or not to reset shaker values after shake
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        /// whether or not to reset the target's values after shake
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Intensity")]
        /// the curve to animate the intensity on
        [Tooltip("the curve to animate the intensity on")]
        public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        /// the value to which to remap the curve's zero to
        [Tooltip("the value to which to remap the curve's zero to")]
        public float RemapIntensityZero = 0f;
        /// the value to which to remap the curve's one to
        [Tooltip("the value to which to remap the curve's one to")]
        public float RemapIntensityOne = 1000f;
        /// whether or not to add to the initial intensity
        [Tooltip("whether or not to add to the initial intensity")]
        public bool RelativeIntensity = false;

        /// the duration of this feedback is the duration of the shake
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        /// <summary>
        /// Triggers a motion blur shake
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMMotionBlurShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
                    Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
    }
}
