using UnityEngine;

namespace ColorBlockJam.Core.SceneManagement
{
    /// <summary>
    /// A scene that is fully loaded in the background and waits for activation.
    /// </summary>
    public readonly struct PreloadedScene
    {
        private readonly AsyncOperation operation;

        public PreloadedScene(AsyncOperation operation)
        {
            this.operation = operation;
        }

        public void Activate()
        {
            operation.allowSceneActivation = true;
        }
    }
}
