using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Core.Startup;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.UI.Windows
{
    public sealed class WindowWarmup : IStartupTask
    {
        private const int RenderedFrames = 2;
        private const int SortingOrderBehindEveryCanvas = short.MinValue;
        private const string SampleText = "0";

        private readonly IReadOnlyList<WindowCatalog> catalogs;

        public WindowWarmup(IReadOnlyList<WindowCatalog> catalogs)
        {
            this.catalogs = catalogs;
        }

        public async UniTask RunAsync(CancellationToken cancellationToken)
        {
            var canvas = new GameObject("Window Warmup").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrderBehindEveryCanvas;
            try
            {
                AddSamples(canvas.transform);
                await UniTask.DelayFrame(RenderedFrames, cancellationToken: cancellationToken);
            }
            finally
            {
                Object.Destroy(canvas.gameObject);
            }
        }

        private void AddSamples(Transform parent)
        {
            var materials = new HashSet<Material>();
            foreach (var catalog in catalogs)
            {
                foreach (var entry in catalog.Windows)
                {
                    foreach (var graphic in entry.prefab.GetComponentsInChildren<Graphic>(true))
                    {
                        var material = graphic is TMP_Text text ? text.fontSharedMaterial : graphic.material;
                        if (material != null && materials.Add(material))
                        {
                            AddSample(parent, graphic, material);
                        }
                    }
                }
            }
        }

        private static void AddSample(Transform parent, Graphic source, Material material)
        {
            var sample = new GameObject(material.name, typeof(RectTransform));
            sample.transform.SetParent(parent, false);
            if (source is TMP_Text text)
            {
                var sampleText = sample.AddComponent<TextMeshProUGUI>();
                sampleText.font = text.font;
                sampleText.fontSharedMaterial = material;
                sampleText.text = SampleText;
                return;
            }

            sample.AddComponent<Image>().material = material;
        }
    }
}
