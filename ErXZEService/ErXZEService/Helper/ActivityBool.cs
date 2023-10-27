using System;

namespace ErXZEService.Helper
{
    public class ActivityBool : IDisposable
    {
        public bool IsActive { get; set; } = true;

        public void Dispose()
        {
            IsActive = false;
        }
    }
}
