using System.Text;
using Linger.Extensions.IO;

namespace Linger.Helper;
public static partial class FileHelper
{
    /// <summary>
    /// 检测指定文件是否存在,如果存在则返回true。
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static bool IsExistFile(string filePath)
    {
        return File.Exists(filePath);
    }

    public static void WriteTxt(string fileName, string content, Encoding? encoding = null)
    {
        var filePath = fileName.GetFilePathString();
        CreateDirectoryIfNotExists(filePath);

        encoding ??= ExtensionMethodSetting.DefaultEncoding;
        StreamWriter? sw = null;
        {
            try
            {
                sw = new StreamWriter(fileName, false, encoding);
                sw.Write(content);
                sw.Flush();
            }
            catch
            {
                // ignored
            }
        }
        if (sw != null)
        {
            sw.Close();
            sw.Dispose();
        }
    }


    /// <summary>
    /// 将源文件的内容复制到目标文件中
    /// </summary>
    /// <param name="sourceFilePath">源文件的绝对路径</param>
    /// <param name="destFilePath">目标文件的绝对路径</param>
    public static void Copy(string sourceFilePath, string destFilePath)
    {
        File.Copy(sourceFilePath, destFilePath, true);
    }

    /// <summary>
    /// 获取文本文件的行数
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static int GetLineCount(string filePath)
    {
        //将文本文件的各行读到一个字符串数组中
        var rows = File.ReadAllLines(filePath);

        //返回行数
        return rows.Length;
    }

    /// <summary>
    /// 获取一个文件的长度,单位为Byte
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static int GetFileSize(string filePath)
    {
        //创建一个文件对象
        var fi = new FileInfo(filePath);

        //获取文件的大小
        return (int)fi.Length;
    }

    public static string ReadTxt(string filename, Encoding? encoding = null)
    {
        encoding ??= ExtensionMethodSetting.DefaultEncoding;
        var str = string.Empty;
        if (File.Exists(filename))
        {
            StreamReader? sr = null;
            try
            {
                sr = new StreamReader(filename, encoding);
                str = sr.ReadToEnd(); // 读取文件
            }
            catch
            {
                // ignored
            }

            if (sr != null)
            {
                sr.Close();
                sr.Dispose();
            }
        }
        else
        {
            str = string.Empty;
        }

        return str;
    }

    /****************************************
     * 函数名称：GetPostfixStr
     * 功能说明：取得文件后缀名
     * 参    数：filename:文件名称
     * 调用示列：
     *           string filename = "aaa.aspx";
     *           string s = DotNet.Utilities.FileOperate.GetPostfixStr(filename);
     *****************************************/

    /// <summary>
    /// 取后缀名
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <returns>.gif|.html格式</returns>
    public static string GetPostfixStr(string filename)
    {
        var start = filename.LastIndexOf(".", StringComparison.Ordinal);
        var length = filename.Length;
        var postfix = filename.Substring(start, length - start);
        return postfix;
    }

    /****************************************
     * 函数名称：GetDirectoryLength(string dirPath)
     * 功能说明：获取文件夹大小
     * 参    数：dirPath:文件夹详细路径
     * 调用示列：
     *           string Path = Server.MapPath("templates");
     *           Response.Write(DotNet.Utilities.FileOperate.GetDirectoryLength(Path));
     *****************************************/

    /// <summary>
    /// 获取文件夹大小
    /// </summary>
    /// <param name="dirPath">文件夹路径</param>
    /// <returns></returns>
    public static long GetDirectoryLength(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            return 0;
        }

        long len = 0;
        var di = new DirectoryInfo(dirPath);
        foreach (FileInfo fi in di.GetFiles())
        {
            len += fi.Length;
        }

        DirectoryInfo[] dis = di.GetDirectories();
        if (dis.Length > 0)
        {
            foreach (DirectoryInfo t in dis)
            {
                len += GetDirectoryLength(t.FullName);
            }
        }

        return len;
    }

    /****************************************
     * 函数名称：GetFileAttribute(string filePath)
     * 功能说明：获取指定文件详细属性
     * 参    数：filePath:文件详细路径
     * 调用示列：
     *           string file = Server.MapPath("robots.txt");
     *            Response.Write(DotNet.Utilities.FileOperate.GetFileAttribute(file));
     *****************************************/

    /// <summary>
    /// 获取指定文件详细属性
    /// </summary>
    /// <param name="filePath">文件详细路径</param>
    /// <returns></returns>
    public static string GetFileAttribute(string filePath)
    {
        var str = string.Empty;
        var objFi = new FileInfo(filePath);
        str += "详细路径:" + objFi.FullName + "<br>文件名称:" + objFi.Name + "<br>文件长度:" + objFi.Length + "字节<br>创建时间" +
               objFi.CreationTime + "<br>最后访问时间:" + objFi.LastAccessTime + "<br>修改时间:" + objFi.LastWriteTime +
               "<br>所在目录:" + objFi.DirectoryName + "<br>扩展名:" + objFi.Extension;
        return str;
    }

    /// <summary>
    /// 检测指定目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录的绝对路径</param>
    /// <returns></returns>
    public static bool IsExistDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    /// <summary>
    /// 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    public static string[] GetDirectories(string directoryPath)
    {
        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// 获取指定目录及子目录中所有子目录列表
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
    /// <param name="isSearchChild">是否搜索子目录</param>
    public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
    {
        if (isSearchChild)
        {
            return Directory.GetDirectories(directoryPath, searchPattern, SearchOption.AllDirectories);
        }

        return Directory.GetDirectories(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// 获取指定目录及子目录中所有文件列表
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
    /// <param name="isSearchChild">是否搜索子目录</param>
    public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
    {
        //如果目录不存在，则抛出异常
        if (!IsExistDirectory(directoryPath))
        {
            throw new FileNotFoundException();
        }

        if (isSearchChild)
        {
            return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        }

        return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">要创建的目录路径包括目录名</param>
    public static void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!IsExistDirectory(directoryPath))
        {
            _ = Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 复制文件夹(递归)
    /// </summary>
    /// <param name="varFromDirectory">源文件夹路径</param>
    /// <param name="varToDirectory">目标文件夹路径</param>
    public static void CopyFolder(string varFromDirectory, string varToDirectory)
    {
        _ = Directory.CreateDirectory(varToDirectory);

        if (!Directory.Exists(varFromDirectory))
        {
            return;
        }

        var directories = Directory.GetDirectories(varFromDirectory);

        if (directories.Length > 0)
        {
            foreach (var d in directories)
            {
                CopyFolder(d, varToDirectory + d.Substring(d.LastIndexOf("\\", StringComparison.Ordinal)));
            }
        }

        var files = Directory.GetFiles(varFromDirectory);
        if (files.Length > 0)
        {
            foreach (var s in files)
            {
                File.Copy(s, varToDirectory + s.Substring(s.LastIndexOf("\\", StringComparison.Ordinal)), true);
            }
        }
    }

    /// <summary>
    /// 删除第二个文件夹里与第一个文件夹共有的文件
    /// </summary>
    /// <param name="varFromDirectory">指定文件夹路径</param>
    /// <param name="varToDirectory">对应其他文件夹路径</param>
    public static void DeleteFolderFiles(string varFromDirectory, string varToDirectory)
    {
        _ = Directory.CreateDirectory(varToDirectory);

        if (!Directory.Exists(varFromDirectory))
        {
            return;
        }

        var directories = Directory.GetDirectories(varFromDirectory);

        if (directories.Length > 0)
        {
            foreach (var d in directories)
            {
                DeleteFolderFiles(d, varToDirectory + d.Substring(d.LastIndexOf("\\", StringComparison.Ordinal)));
            }
        }

        var files = Directory.GetFiles(varFromDirectory);

        if (files.Length > 0)
        {
            foreach (var s in files)
            {
                File.Delete(varToDirectory + s.Substring(s.LastIndexOf("\\", StringComparison.Ordinal)));
            }
        }
    }

    /// <summary>
    /// 从文件的绝对路径中获取文件名( 包含扩展名 )
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetFileName(string filePath)
    {
        //获取文件的名称
        var fi = new FileInfo(filePath);
        return fi.Name;
    }

    /****************************************
     * 函数名称：GetFoldAll(string Path)
     * 功能说明：获取指定文件夹下所有子目录及文件(树形)
     * 参    数：Path:详细路径
     * 调用示列：
     *           string strDirList = Server.MapPath("templates");
     *           this.Literal1.Text = DotNet.Utilities.FileOperate.GetFoldAll(strDirList);
     *****************************************/

    /// <summary>
    /// 获取指定文件夹下所有子目录及文件
    /// </summary>
    /// <param name="path">详细路径</param>
    public static string GetFoldAll(string path)
    {
        var str = string.Empty;
        var thisOne = new DirectoryInfo(path);
        str = ListTreeShow(thisOne, 0, str);
        return str;
    }

    /// <summary>
    /// 获取指定文件夹下所有子目录及文件函数
    /// </summary>
    /// <param name="theDir">指定目录</param>
    /// <param name="nLevel">默认起始值,调用时,一般为0</param>
    /// <param name="rn">用于迭加的传入值,一般为空</param>
    /// <returns></returns>
    public static string ListTreeShow(DirectoryInfo theDir, int nLevel, string rn) //递归目录 文件
    {
        DirectoryInfo[] subDirectories = theDir.GetDirectories(); //获得目录
        foreach (DirectoryInfo dirInfo in subDirectories)
        {
            if (nLevel == 0)
            {
                rn += "├";
            }
            else
            {
                var s = string.Empty;
                for (var i = 1; i <= nLevel; i++)
                {
                    s += "│ ";
                }

                rn += s + "├";
            }

            rn += "<b>" + dirInfo.Name + "</b><br />";
            FileInfo[] fileInfo = dirInfo.GetFiles(); //目录下的文件
            foreach (FileInfo fInfo in fileInfo)
            {
                if (nLevel == 0)
                {
                    rn += "│ ├";
                }
                else
                {
                    var f = string.Empty;
                    for (var i = 1; i <= nLevel; i++)
                    {
                        f += "│ ";
                    }

                    rn += f + "│ ├";
                }

                rn += fInfo.Name + " <br />";
            }

            rn = ListTreeShow(dirInfo, nLevel + 1, rn);
        }

        return rn;
    }

    /****************************************
     * 函数名称：GetFoldAll(string Path)
     * 功能说明：获取指定文件夹下所有子目录及文件(下拉框形)
     * 参    数：Path:详细路径
     * 调用示列：
     *            string strDirList = Server.MapPath("templates");
     *            this.Literal2.Text = DotNet.Utilities.FileOperate.GetFoldAll(strDirList,"tpl","");
     *****************************************/

    /// <summary>
    /// 获取指定文件夹下所有子目录及文件(下拉框形)
    /// </summary>
    /// <param name="path">详细路径</param>
    /// <param name="dropName">下拉列表名称</param>
    /// <param name="tplPath">默认选择模板名称</param>
    public static string GetFoldAll(string path, string dropName, string tplPath)
    {
        var strDrop = "<select name=\"" + dropName + "\" id=\"" + dropName +
                      "\"><option value=\"\">--请选择详细模板--</option>";
        var str = string.Empty;
        var thisOne = new DirectoryInfo(path);
        str = ListTreeShow(thisOne, 0, str, tplPath);
        return strDrop + str + "</select>";
    }

    /// <summary>
    /// 获取指定文件夹下所有子目录及文件函数
    /// </summary>
    /// <param name="theDir">指定目录</param>
    /// <param name="nLevel">默认起始值,调用时,一般为0</param>
    /// <param name="rn">用于迭加的传入值,一般为空</param>
    /// <param name="tplPath">默认选择模板名称</param>
    /// <returns></returns>
    public static string ListTreeShow(DirectoryInfo theDir, int nLevel, string rn, string tplPath) //递归目录 文件
    {
        DirectoryInfo[] subDirectories = theDir.GetDirectories(); //获得目录

        foreach (DirectoryInfo dirInfo in subDirectories)
        {
            rn += "<option value=\"" + dirInfo.Name + "\"";
            if (tplPath.Equals(dirInfo.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                rn += " selected ";
            }

            rn += ">";

            if (nLevel == 0)
            {
                rn += "┣";
            }
            else
            {
                var s = string.Empty;
                for (var i = 1; i <= nLevel; i++)
                {
                    s += "│ ";
                }

                rn += s + "┣";
            }

            rn += string.Empty + dirInfo.Name + "</option>";

            FileInfo[] fileInfo = dirInfo.GetFiles(); //目录下的文件
            foreach (FileInfo fInfo in fileInfo)
            {
                rn += "<option value=\"" + dirInfo.Name + "/" + fInfo.Name + "\"";
                if (tplPath.Equals(fInfo.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    rn += " selected ";
                }

                rn += ">";

                if (nLevel == 0)
                {
                    rn += "│ ├";
                }
                else
                {
                    var f = string.Empty;
                    for (var i = 1; i <= nLevel; i++)
                    {
                        f += "│ ";
                    }

                    rn += f + "│ ├";
                }

                rn += fInfo.Name + "</option>";
            }

            rn = ListTreeShow(dirInfo, nLevel + 1, rn, tplPath);
        }

        return rn;
    }

    /// <summary>
    /// 检测指定目录中是否存在指定的文件,若要搜索子目录请使用重载方法.
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
    public static bool Contains(string directoryPath, string searchPattern)
    {
        try
        {
            //获取指定的文件列表
            var fileNames = GetFileNames(directoryPath, searchPattern, false);

            //判断指定文件是否存在
            if (fileNames.Length == 0)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
            //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
        }
    }

    /// <summary>
    /// 根据时间得到目录名yyyyMMdd
    /// </summary>
    /// <returns></returns>
    public static string GetDateDir()
    {
        return DateTime.Now.ToString("yyyyMMdd");
    }

    /// <summary>
    /// 根据时间得到文件名HHmmssff
    /// </summary>
    /// <returns></returns>
    public static string GetDateFile()
    {
        return DateTime.Now.ToString("HHmmssff");
    }
}
