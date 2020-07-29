using System;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class InterceptUrlAction
    {
        public Func<string, bool> Action { get; }

        public InterceptUrlAction(Func<string, bool> action)
        {
            Action = action;
        }
    }
}