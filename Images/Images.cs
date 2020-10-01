using System;
using Exiled.API.Features;
using Exiled.API.Interfaces;

namespace Images
{
    public class Images : Plugin<Config>
    {
        public override string Name { get; } = "Images";
        public override string Author { get; } = "PintTheDragon";
        public override Version Version { get; } = new Version(1, 0, 0);

        public static Images Singleton;

        public override void OnEnabled()
        {
            base.OnEnabled();

            Singleton = this;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Singleton = null;
        }
    }
}