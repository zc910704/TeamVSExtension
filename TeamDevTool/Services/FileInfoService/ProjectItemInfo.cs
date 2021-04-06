using EnvDTE;
using TeamDevTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.Services.FileInfoService
{
    /// <summary>
    /// ProjectItem包裹类型，ProjectItem是COM对象
    /// </summary>
    public class ProjectItemInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public ProjectItemType ItemType { get; set; }

        /// <summary>
        /// 父文件夹，项目根目录父文件夹为null
        /// </summary>
        public ProjectItemInfo Parent { get; set; }

        /// <summary>
        /// 自身
        /// </summary>
        public ProjectItem Self { get; set; }

        /// <summary>
        /// 子文件或文件夹
        /// </summary>
        public IList<ProjectItemInfo> Children { get; set; }

        /// <summary>
        /// 所属项目
        /// </summary>
        public Project Project { get; set; }
    }


}
