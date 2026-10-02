using System;
using ColorBlockJam.UI.Transitions;
using UnityEngine;

namespace ColorBlockJam.UI.Windows
{
    [Serializable]
    public sealed class WindowSettings
    {
        [Tooltip("Pencere açıkken arkası karartılır.")]
        public bool dimsBackground = true;
        [Tooltip("Arkadaki karartmaya dokunmak pencereyi kapatır.")]
        public bool closeOnBackdropClick = true;
        [Tooltip("Android geri tuşu pencereyi kapatır.")]
        public bool closeOnBackButton = true;
        [Tooltip("Sahne değişince pencere açıksa kendiliğinden kapanır.")]
        public bool closeOnSceneChange = true;
        [Tooltip("Pencere kapanınca örneği yok edilir. Nadiren açılan pencerelerde bellek boşaltmak için.")]
        public bool destroyOnHide;
        [Tooltip("Pencere açılırken oynayan geçiş. Boş bırakılırsa anında görünür.")]
        public ViewTransition showTransition;
        [Tooltip("Pencere kapanırken oynayan geçiş. Boş bırakılırsa anında kaybolur.")]
        public ViewTransition hideTransition;
    }
}
