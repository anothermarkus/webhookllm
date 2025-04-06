using System.Text.Json.Serialization;

namespace GitLabWebhook.Models
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StrategyType
    {
        ZeroShot,
        FewShot,
        Embedding,
        Finetune,
        ChainOfThought,
        Toolformer


    }
}