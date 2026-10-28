#if GEOAI_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.GeoAi;

internal sealed class GeoAiRenderFence
{
    private bool busy;

    public GeoAiRenderFence(GeoKernelViewerControl viewer)
    {
        viewer.BusyChanged += (_, e) => busy = e.Busy;
    }

    // Called on the owning UI thread after detaching result layers. Keep pumping UI/native events
    // until render completion has released retired layer/provider objects and their open files.
    public async Task WaitAsync()
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(60);
        do
        {
            await Task.Delay(16);
            if (!busy)
            {
                await Task.Yield();
                if (!busy) return;
            }
        } while (DateTime.UtcNow < deadline);
        throw new TimeoutException("Renderer is still using the result; cleanup could not finish.");
    }
}
