using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.Services.PluginInfoService
{
    /// <summary>
    /// interface for query service
    /// </summary>
    public interface SPluginInfoService { }

    /// <summary>
    /// interface for service behavior
    /// </summary>
    interface IPluginInfoService
    {
        event Func<PluginInfo> ActivePluginChanged;
    }
}
