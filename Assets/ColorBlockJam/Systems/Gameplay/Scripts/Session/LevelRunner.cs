using System;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelRunner : IStartable, ITickable, IDisposable
    {
        private readonly LifetimeScope owner;
        private readonly IInstaller levelInstaller;
        private LifetimeScope level;
        private bool isRestartPending;

        public LevelRunner(IObjectResolver resolver, IInstaller levelInstaller)
        {
            owner = (LifetimeScope)resolver.ApplicationOrigin;
            this.levelInstaller = levelInstaller;
        }

        public void Start()
        {
            Open();
        }

        public void Restart()
        {
            isRestartPending = true;
        }

        public void Tick()
        {
            if (!isRestartPending)
            {
                return;
            }

            isRestartPending = false;
            Open();
        }

        public void Dispose()
        {
            Close();
        }

        private void Open()
        {
            Close();
            level = owner.CreateChild(levelInstaller, "Level");
        }

        private void Close()
        {
            if (level != null)
            {
                level.Dispose();
            }

            level = null;
        }
    }
}
