using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Services
{
    public class EnvironmentVariableService
    {
        private const string PathVariableName = "PATH";

        public string? ReadPath()
        {
            return Environment.GetEnvironmentVariable(PathVariableName, EnvironmentVariableTarget.Machine);
        }

        public void WritePath(string newPath)
        {
            string? currentPath = ReadPath();

            // 如果当前路径不包含新路径，则添加
            if (!string.IsNullOrEmpty(currentPath) && !currentPath.Contains(newPath))
            {
                string updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable(PathVariableName, updatedPath, EnvironmentVariableTarget.Machine);
                Console.WriteLine("系统环境变量 PATH 已更新");
            }
            else
            {
                Console.WriteLine("路径已存在于环境变量 PATH 中");
            }
        }
    }

}
