using System;

namespace OmniSharp
{
    public static class PlatformHelper
    {
        private static Lazy<bool> _isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

        public static bool IsMono = _isMono.Value;
    }
}
