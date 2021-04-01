using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.Services.FileInfoService
{
    /*  实体服务必须实现以下两种接口 */

    /// <summary>
    /// 服务的行为与细节接口
    /// </summary>
    public interface IFileInfoService
    {
        /// <summary>
        /// 获取当前编辑工程
        /// </summary>
        /// <returns></returns>
        ProjectItemInfo GetProjectItemInfo(ProjectItem projectItem);

        IEnumerable<ProjectItemInfo> GetProjectItemByProject(Project project);
    }

    /// <summary>
    /// 仅用作外部用来查询的提供何种服务
    /// </summary>
    public interface SFileInfoService
    {

    }
}
