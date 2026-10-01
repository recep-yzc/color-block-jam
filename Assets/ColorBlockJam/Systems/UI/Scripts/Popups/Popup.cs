using System;
using ColorBlockJam.UI.Views;
using UnityEngine;

namespace ColorBlockJam.UI.Popups
{
    public class Popup : UIView
    {
        [Tooltip("Arkadaki karartmaya dokunmak popup'ı kapatır.")]
        [SerializeField] private bool closeOnBackdropClick = true;
        [Tooltip("Android geri tuşu popup'ı kapatır.")]
        [SerializeField] private bool closeOnBackButton = true;
        [Tooltip("Popup kapanınca örneği yok edilir. Nadiren açılan popup'larda bellek boşaltmak için.")]
        [SerializeField] private bool destroyOnHide;

        public event Action<Popup> CloseRequested;

        public bool CloseOnBackdropClick => closeOnBackdropClick;
        public bool CloseOnBackButton => closeOnBackButton;
        public bool DestroyOnHide => destroyOnHide;

        public void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }
    }
}
