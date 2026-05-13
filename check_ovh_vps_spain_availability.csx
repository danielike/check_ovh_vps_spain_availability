#!/usr/bin/env dotnet-script
#r "nuget: Telegram.Bot, 22.10.0"
#r "nuget: dotenv.net, 3.1.0"

using dotenv.net;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Linq;

public static class OvhVpsStatus
{
    public const string Available = "available";
    public const string OutOfStock = "out-of-stock";
}

public const string SpainVPS = "EU-SOUTH-LZ-MAD";

public class Root
{    
    [JsonPropertyName("ok")]    
    public bool Ok { get; set; }
    
    [JsonPropertyName("result")]    
    public List<Update> Result { get; set; }
}

public class Update
{    
    [JsonPropertyName("update_id")]    
    public long UpdateId { get; set; }

    [JsonPropertyName("message")]    
    public Message Message { get; set; }
}
    
public class Message
{    
    [JsonPropertyName("message_id")]    
    public int MessageId { get; set; }
    
    [JsonPropertyName("from")]    
    public From From { get; set; }
    
    [JsonPropertyName("chat")]    
    public Chat Chat { get; set; }
    
    [JsonPropertyName("date")]    
    public long Date { get; set; }
    
    [JsonPropertyName("text")]    
    public string Text { get; set; }
}
    
public class From
{
    [JsonPropertyName("id")]    
    public long Id { get; set; }
    
    [JsonPropertyName("is_bot")]    
    public bool IsBot { get; set; }
    
    [JsonPropertyName("first_name")]    
    public string FirstName { get; set; }
    
    [JsonPropertyName("last_name")]    
    public string LastName { get; set; }
    
    [JsonPropertyName("username")]    
    public string Username { get; set; }
    
    [JsonPropertyName("language_code")]    
    public string LanguageCode { get; set; }
}

public class Chat
{    
    [JsonPropertyName("id")]    
    public long Id { get; set; }

    [JsonPropertyName("first_name")]    
    public string FirstName { get; set; }
    
    [JsonPropertyName("last_name")]    
    public string LastName { get; set; }
    
    [JsonPropertyName("username")]    
    public string Username { get; set; }
    
    [JsonPropertyName("type")]    
    public string Type { get; set; }
}

public class DataCenterStatus
{
    public List<DataCenterItem> datacenters { get; set; }
}

public class DataCenterItem
{
    public string datacenter { get; set; }
    public string code { get; set; }
    public string status { get; set; }
    public int daysBeforeDelivery { get; set; }
    public string linuxStatus { get; set; }
    public string windowsStatus { get; set; }
}

DotEnv.Load();
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new Exception("TELEGRAM_BOT_TOKEN not set");

var botClient = new TelegramBotClient(token);
var httpClient = new HttpClient();

var root = await httpClient.GetFromJsonAsync<Root>($"https://api.telegram.org/bot{token}/getUpdates");
var chatId = root.Result.FirstOrDefault()?.Message.Chat.Id;

Console.WriteLine("Service started.");
while (true)
{
    var currentData = await httpClient.GetFromJsonAsync<DataCenterStatus>("https://www.ovhcloud.com/eu/engine/api/v1/vps/order/rule/datacenter/?ovhSubsidiary=ES&os=Ubuntu%2025.04&planCode=vps-2025-model1.LZ");
    
    foreach (var item in currentData.datacenters)
    {
        if (item.datacenter == SpainVPS && item.linuxStatus == OvhVpsStatus.Available)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Spain VPS is available in OVHCloud!\n",
                ParseMode.Html,
                protectContent: true,
                replyMarkup: new InlineKeyboardButton("Open Ovh website configurator", "https://www.ovhcloud.com/es-es/vps/configurator/?planCode=vps-2025-model1&brick=VPS%2BModel%2B1&pricing=upfront12&processor=%20&vcore=4__vCore&storage=75__SSD__NVMe")
            );
            break;
        }
    }
    await Task.Delay(TimeSpan.FromSeconds(5)); // Check every 30 minutes
}
