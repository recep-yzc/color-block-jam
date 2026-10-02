using System;
using UnityEngine;

namespace ColorBlockJam.UI.Windows
{
    [Serializable]
    public sealed class WindowEntry
    {
        [Tooltip("Pencereyi açan presenter. Bu presenter istendiğinde aşağıdaki prefab üretilir.")]
        [WindowPresenterType]
        public string presenter;
        [Tooltip("Presenter'ın gösterdiği prefab. Sadece görünümün referanslarını ve kendine özel ayarlarını tutar.")]
        public WindowView prefab;
        [Tooltip("Bu pencerenin nasıl açılıp kapandığı.")]
        public WindowSettings settings = new();

        public Type PresenterType => string.IsNullOrEmpty(presenter) ? null : Type.GetType(presenter);
    }
}
