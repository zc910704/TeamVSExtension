using TeamDevTool.Services.FileInfoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.Services.PluginInfoService
{
    public class PluginInfo
    {
        public string ComponentID { get; set; }

        public ProjectItemInfo IssueProjectItemInfo { get; set; }

        public ProjectPluginInfo ProjectPluginInfo { get; set; }
    }

    public class ProjectPluginInfo
    {
        public ProjectItemInfo ProjecIssuetCommonInfo { get; set; }

        public List<PluginInfo> PluginIssueInfo { get; set; } = new List<PluginInfo>();


    }
}
