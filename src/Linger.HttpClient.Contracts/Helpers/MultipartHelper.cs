namespace Linger.HttpClient.Contracts.Helpers;

/// <summary>
/// 多部分内容处理助手
/// </summary>
public static class MultipartHelper
{
    /// <summary>
    /// 创建多部分表单内容，包含文件和表单数据
    /// </summary>
    /// <param name="formData">表单数据</param>
    /// <param name="fileData">文件数据</param>
    /// <param name="fileName">文件名</param>
    /// <param name="fileFieldName">文件字段名，默认为"file"</param>
    /// <returns>多部分表单内容</returns>
    public static MultipartFormDataContent CreateMultipartContent(
        IDictionary<string, string>? formData,
        byte[] fileData,
        string fileName,
        string fileFieldName = "file")
    {
        var content = new MultipartFormDataContent();

        // 添加文件
        var fileContent = new ByteArrayContent(fileData);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        // 根据文件扩展名设置内容类型
        string contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, fileFieldName, fileName);

        // 添加表单数据
        if (formData != null)
        {
            foreach (var field in formData)
            {
                content.Add(new StringContent(field.Value), field.Key);
            }
        }

        return content;
    }
}
