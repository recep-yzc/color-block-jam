using ColorBlockJam.UI;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Navigation
{
    public sealed class PunchOnPageOpening : MonoBehaviour, IPageOpeningListener
    {
        [Tooltip("Sayfa açılınca zıplatılan obje.")]
        [SerializeField] private Transform target;
        [Tooltip("Zıplamanın büyüklüğü, ölçeğe oranla.")]
        [SerializeField] private float strength = 0.12f;
        [Tooltip("Zıplamanın süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float duration = 0.45f;
        [Tooltip("Zıplama boyunca salınım sayısı.")]
        [SerializeField, Min(1)] private int frequency = 5;

        private MotionHandle punch;
        private Vector3 restScale;

        private void Awake()
        {
            restScale = target.localScale;
        }

        public void OnPageOpening()
        {
            punch.TryCancel();
            punch = LMotion.Punch.Create(restScale, restScale * strength, duration)
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
