using System.Net;
using System.Text;
using System.Text.Json;
using MyChatBot;


var apiEndpoint = "http://127.0.0.1:11434";
var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiEndpoint)
};
var modelName =  await SelectModelAsync(httpClient);

await InitiateChatAsync(httpClient, modelName);
Console.WriteLine("Exiting the application.");

static async Task<bool> ProcessUserInputAsync(HttpClient httpClient, ChatRequest chatRequest)
{
    Console.Write("User > ");

    var continueConversation = true;
    var userInput = Console.ReadLine();

    if (userInput == "/bye" || string.IsNullOrWhiteSpace(userInput))
    {
        return continueConversation = false;
    }

    ChatResponse? chatResponse = await GetChatResponseAsync(httpClient, chatRequest, userInput);

    if (chatResponse != null)
    {
        var assistantMessage = new Message { Role = chatResponse.Message.Role, Content = chatResponse.Message.Content };
        chatRequest.Messages.Add(assistantMessage);
        Console.WriteLine($"{assistantMessage.Role} > {assistantMessage.Content}");
    }
    else
    {
        Console.WriteLine("Failed to process the response.");
        continueConversation = false;
    }

    return continueConversation;
}

static async Task<string> SelectModelAsync(HttpClient httpClient)
{
    var response = await httpClient.GetAsync("/api/tags");
    var responseBody = await response.Content.ReadAsStringAsync();

    if (response != null && response.StatusCode == HttpStatusCode.OK && responseBody != null)
    {
        var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(responseBody);

        if (modelsResponse != null)
        {
            for (int i = 0; i < modelsResponse.Models.Count; i++)
            {
                Console.WriteLine($"({i}) {modelsResponse.Models[i].Name}");
            }

            Console.WriteLine("\nSelect a model by entering its corresponding number:");

            var userInput = Console.ReadLine();

            if (int.TryParse(userInput, out int modelIndex) && modelIndex >= 0 && modelIndex < modelsResponse.Models.Count)
            {
                return modelsResponse.Models[modelIndex].Name;
            }

            Console.WriteLine("Invalid model selection.");
        }
    }

    return string.Empty;
}

static async Task InitiateChatAsync(HttpClient httpClient, string modelName)
{
    if (!string.IsNullOrEmpty(modelName))
    {
        Console.WriteLine("Hello! I'm your AI assistant. How can I assist you today?");
        Console.WriteLine("To exit, type /bye\n");

        var chatRequest = new ChatRequest
        {
            Model = modelName,
            Messages = new List<Message> { new Message { Role = "system", Content = "You are a helpful assistant." } },
            Stream = false
        };

        while (await ProcessUserInputAsync(httpClient, chatRequest));
    }
}

static async Task<ChatResponse?> GetChatResponseAsync(HttpClient httpClient, ChatRequest chatRequest, string userInput)
{
    var userMessage = new Message { Role = "user", Content = userInput };
    chatRequest.Messages.Add(userMessage);

    var requestBody = JsonSerializer.Serialize(chatRequest);
    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync("/api/chat", content);
    var responseBody = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<ChatResponse>(responseBody);
}
