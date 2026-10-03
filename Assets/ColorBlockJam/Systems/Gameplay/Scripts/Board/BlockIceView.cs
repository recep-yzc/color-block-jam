using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class BlockIceView : MonoBehaviour
    {
        [Tooltip("Bloğun mesh'ini saran yarı saydam buz kabuğu. Mesh'i blok kurulurken verilir, materyali prefab'dadır.")]
        [SerializeField] private MeshFilter shell;
        [Tooltip("Buzun kırılması için kaç blok daha çıkması gerektiğini gösteren sayı.")]
        [SerializeField] private TMP_Text count;

        private MotionHandle motion;

        public void Show(Mesh blockMesh, Vector3 countPosition, int left)
        {
            shell.sharedMesh = blockMesh;
            count.transform.localPosition = countPosition;
            count.SetText("{0}", left);
        }

        public void ShowLeft(int left, float punch, float duration)
        {
            motion.TryComplete();
            count.SetText("{0}", left);
            motion = LMotion.Punch.Create(Vector3.one, Vector3.one * punch, duration)
                .BindToLocalScale(count.transform)
                .AddTo(this);
        }

        public void Break(float duration)
        {
            motion.TryComplete();
            count.gameObject.SetActive(false);
            motion = LMotion.Create(Vector3.one, Vector3.zero, duration)
                .WithEase(Ease.InBack)
                .WithOnComplete(Remove)
                .BindToLocalScale(shell.transform)
                .AddTo(this);
        }

        private void Remove()
        {
            Destroy(gameObject);
        }
    }
}
