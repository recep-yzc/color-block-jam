using ColorBlockJam.Shared.UI;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Shared.Navigation
{
    /// <summary>
    /// Gives the target a short scale punch each time its page starts to open, to draw the eye to it.
    /// </summary>
    public sealed class PunchOnPageOpening : MonoBehaviour, IPageOpeningListener
    {
        [SerializeField] private Transform target;
        [SerializeField] private float strength = 0.12f;
        [SerializeField, Min(0.01f)] private float duration = 0.45f;
        [SerializeField, Min(1)] private int frequency = 5;

        private MotionHandle punch;

        public void OnPageOpening()
        {
            punch.TryCancel();
            punch = LMotion.Punch.Create(Vector3.one, Vector3.one * strength, duration)
                .WithFrequency(frequency)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);
        }

        private void OnDestroy()
        {
            punch.TryCancel();
        }
    }
}
