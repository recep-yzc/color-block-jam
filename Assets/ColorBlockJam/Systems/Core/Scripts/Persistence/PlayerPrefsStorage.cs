using System;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class PlayerPrefsStorage : IKeyValueStorage, IDisposable
    {
        private bool hasUnsavedValues;

        public PlayerPrefsStorage()
        {
            Application.focusChanged += OnFocusChanged;
            Application.quitting += Flush;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        public void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            hasUnsavedValues = true;
        }

        public void Dispose()
        {
            Application.focusChanged -= OnFocusChanged;
            Application.quitting -= Flush;
            Flush();
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                Flush();
            }
        }

        private void Flush()
        {
            if (!hasUnsavedValues)
            {
                return;
            }

            hasUnsavedValues = false;
            PlayerPrefs.Save();
        }
    }
}
