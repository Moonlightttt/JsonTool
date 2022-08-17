using System.Text.RegularExpressions;
using DevelopmentTool.Web.Controllers.Inputs;
using DevelopmentTool.Web.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DevelopmentTool.Web.Controllers;

/// <summary>
/// 工具集
/// </summary>
[ApiController]
[Route("[controller]/[Action]")]
public class ToolController : ControllerBase
{
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<ToolController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    public ToolController(ILogger<ToolController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// json属性名转换Camel格式
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [HttpPost(Name = "ConvertToCamel")]
    public string ConvertToCamel(ConvertToCamelInputs inputs)
    {
        _logger.LogInformation($"Json转换请求：{inputs.JsonStr}");
        JTokenWriter writer = new JTokenWriter();
        var jToken = JToken.Parse(inputs.JsonStr!);

        ProcessJson(jToken, writer);

        var result = writer.Token!.ToString();
        _logger.LogInformation($"Json转换响应：{inputs.JsonStr}");
        return result;
    }

    /// <summary>
    /// 查询Token
    /// </summary>
    /// <returns></returns>
    [HttpPost(Name = "QueryRedisValue")]
    public IActionResult QueryRedisValue()
    {
        var value = RedisManager.Instance.HashGet(
            "c:EcommerceCloud.ApiClient.Filters.ParkApiActionFilter+ParkTokenInfo,k:ParkTokenCacheKay", "data");

        var jObject = JObject.Parse(value!);
        
         _logger.LogInformation($"获取Redis值：{jObject}");

        return Ok(new { token = jObject["token"]!.Value<string>() });
    }

    private void ProcessJson(JToken jToken, JTokenWriter writer)
    {
        switch (jToken)
        {
            case JArray a:
                writer.WriteStartArray();
                foreach (var item in a)
                {
                    ProcessJson(item, writer);
                }

                writer.WriteEndArray();
                break;
            case JObject o:
                writer.WriteStartObject();
                foreach (var item in o.Children())
                {
                    ProcessJson(item, writer);
                }

                writer.WriteEndObject();
                break;
            case JProperty p:
                var firstChar = p.Name[0];
                var pName = Regex.Replace(p.Name, "^[A-Za-z]", firstChar.ToString().ToLower());
                writer.WritePropertyName(pName);
                ProcessJson(p.Value, writer);
                break;
            case JValue p:
                writer.WriteValue(p.Value);
                break;
            default:
                throw new Exception("未处理异常");
        }
    }
}