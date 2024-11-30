﻿namespace FlybyScript
{
    public abstract class FeatureBase
    {
        protected readonly Logger logger;

        public Logger Logger => logger;

        protected FeatureBase(Logger logger)
        {
            this.logger = logger;
        }

        public abstract string ID();

        public abstract string Info();

        public abstract bool CheckFeature();

        public abstract bool DoFeature();

        public abstract bool UndoFeature();

        public abstract string GetRegistryKey();
    }
}