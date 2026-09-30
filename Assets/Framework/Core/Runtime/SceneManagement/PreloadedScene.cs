using UnityEngine;

namespace Framework.Core.SceneManagement
{
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
