// Guids.cs
// MUST match guids.h
using System;

namespace Sando.UI
{
    static class GuidList
    {
        public const string guidUIPkgString = "7e03caf3-06ed-4ff5-962a-effa1fb2f383";
        public const string guidUICmdSetString = "61e80ffa-f99b-46ac-8dd0-f3f4171568f3";
        public const string guidToolWindowPersistanceString = "ac71d0b7-7613-4edd-95cc-9be31c0a993a";

        public static readonly Guid guidUICmdSet = new Guid(guidUICmdSetString);
    };
}