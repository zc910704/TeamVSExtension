using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.Utils
{
    public enum ProjectItemType
    {
        PhysicalFile,
        PhysicalFolder,
        SolutionFolder,
        SolutionItem,
        SolutionExplorer
    }

    internal static class VsConstants
    {
        // Reference: https://github.com/umbraco/Visual-Studio-Extension/blob/main/UmbracoStudio/VsConstants.cs

        // Project type guids
        internal const string WebApplicationProjectTypeGuid = "{349C5851-65DF-11DA-9384-00065B846F21}";
        internal const string WebSiteProjectTypeGuid = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        internal const string CsharpProjectTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        internal const string VbProjectTypeGuid = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        internal const string CppProjectTypeGuid = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
        internal const string FsharpProjectTypeGuid = "{F2A71F9B-5D33-465A-A702-920D77279786}";
        internal const string JsProjectTypeGuid = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
        internal const string WixProjectTypeGuid = "{930C7802-8A8C-48F9-8165-68863BCCD9DD}";
        internal const string LightSwitchProjectTypeGuid = "{ECD6D718-D1CF-4119-97F3-97C25A0DFBF9}";
        internal const string NemerleProjectTypeGuid = "{edcc3b85-0bad-11db-bc1a-00112fde8b61}";
        internal const string InstallShieldLimitedEditionTypeGuid = "{FBB4BD86-BF63-432a-A6FB-6CF3A1288F83}";
        internal const string WindowsStoreProjectTypeGuid = "{BC8A1FFA-BEE3-4634-8014-F334798102B3}";
        internal const string SynergexProjectTypeGuid = "{BBD0F5D1-1CC4-42fd-BA4C-A96779C64378}";
        internal const string NomadForVisualStudioProjectTypeGuid = "{4B160523-D178-4405-B438-79FB67C8D499}";

        // Copied from EnvDTE.Constants since that type can't be embedded
        internal const string VsProjectItemKindPhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        internal const string VsProjectItemKindPhysicalFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";
        internal const string VsProjectItemKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        internal const string VsProjectItemKindSolutionItem = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";
        internal const string VsWindowKindSolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}";

        // All unloaded projects have this Kind value
        internal const string UnloadedProjectTypeGuid = "{67294A52-A4F0-11D2-AA88-00C04F688DDE}";

        // HResults
        internal const int S_OK = 0;

        internal static readonly Dictionary<string, ProjectItemType> ProjectItemTypeDict = new Dictionary<string, ProjectItemType>()
        {
            {"{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", ProjectItemType.PhysicalFile},
            {"{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", ProjectItemType.PhysicalFolder},
            {"{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", ProjectItemType.SolutionFolder},
            {"{66A26722-8FB5-11D2-AA7E-00C04F688DDE}", ProjectItemType.SolutionItem},
        };
    }
}
