using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Communication;
using Azure.Communication.CallAutomation;
using Azure.Communication.Sms;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using cakeShopMinimalApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build()
    ?? throw new InvalidOperationException("Configuration is not provided.");

var aiDeploymentName = config["OPENAI_DEPLOYMENT_NAME"] ?? throw new InvalidOperationException("OPENAI_DEPLOYMENT_NAME is not provided.");
var openAIModel = config["OPENAI_MODEL"] ?? throw new InvalidOperationException("OPENAI_MODEL is not provided.");
var openAIAPIKey = config["OPENAI_KEY"] ?? throw new InvalidOperationException("OPENAI_KEY is not provided.");
var openAIUrl = config["OPENAI_ENDPOINT"] ?? throw new InvalidOperationException("OPENAI_ENDPOINT is not provided.");

var aiSearchKey = config["AI_SEARCH_KEY"] ?? throw new InvalidOperationException("AI_SEARCH_KEY is not provided.");
var aiSearchEndpoint = config["AI_SEARCH_ENDPOINT"] ?? throw new InvalidOperationException("AI_SEARCH_ENDPOINT is not provided.");
var aiSearchIndex = config["AI_SEARCH_INDEXNAME"] ?? throw new InvalidOperationException("AI_SEARCH_INDEXNAME is not provided.");

var acsConnectionString = config["ACS_CONNECTION_STRING"] ?? throw new InvalidOperationException("ACS_CONNECTION_STRING is not provided.");
var cognitiveServicesEndpoint = config["AZURE_COG_SERVICES_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_COG_SERVICES_ENDPOINT");
var callbackUriHostString = config["CALLBACK_URI"] ?? throw new InvalidOperationException("CALLBACK_URI is not provided.");

PhoneNumberIdentifier caller = new(config["ACS_PHONE_NUMBER"] ?? throw new InvalidOperationException("ACS_PHONE_NUMBER is not provided."));
var speechVoiceName = "en-US-JennyMultilingualV2Neural";

// Get Azure Open AI chat client
AzureOpenAIClient _aiClient = new AzureOpenAIClient(
    new Uri(openAIUrl),
    new System.ClientModel.ApiKeyCredential(openAIAPIKey)
);

ChatClient chatClient = _aiClient.GetChatClient(aiDeploymentName);

// Setting up the AI search index that has the cake shop knowledgebase as options for chat completion API
ChatCompletionOptions options = new();
options.AddDataSource(new AzureSearchChatDataSource()
{
    Endpoint = new Uri(aiSearchEndpoint),
    IndexName = aiSearchIndex,
    Authentication = DataSourceAuthentication.FromApiKey(aiSearchKey)
});

Kernel kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(openAIModel, openAIAPIKey)
    .Build();

kernel.Plugins.AddFromObject(new CakeOrderPlugin(acsConnectionString, caller.ToString()), "CakeOrderPlugin");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

ConcurrentDictionary<string, List<ChatMessage>> chatHistoryCache = new();
var callAutomationClient = new CallAutomationClient(acsConnectionString);

// Sentence end symbols for splitting the response into sentences.
List<string> sentenceSaperators = new() { ".", "!", "?", ";", "。", "！", "？", "；", "\n" };

// Handle incoming call
app.MapPost("/api/event", async ([FromBody] EventGridEvent[] eventGridEvents) =>
{
    foreach (var eventGridEvent in eventGridEvents)
    {
        // Handle system events
        if (eventGridEvent.TryGetSystemEventData(out object eventData))
        {
            // Handle the subscription validation event
            if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
            {
                var responseData = new SubscriptionValidationResponse
                {
                    ValidationResponse = subscriptionValidationEventData.ValidationCode
                };
                return Results.Ok(responseData);
            }
            else if (eventGridEvent.EventType == "Microsoft.Communication.IncomingCall")
            {
                if (eventData is AcsIncomingCallEventData incomingCallEventData)
                {
                    var encodedCallerId = WebUtility.UrlEncode(incomingCallEventData.FromCommunicationIdentifier.PhoneNumber.Value);
                    var contextId = Guid.NewGuid().ToString();
                    var callbackUri = new Uri(new Uri(callbackUriHostString), $"/api/callbacks/{contextId}?callerId={encodedCallerId}");
                    var options = new AnswerCallOptions(incomingCallEventData.IncomingCallContext, callbackUri)
                    {
                        CallIntelligenceOptions = new CallIntelligenceOptions() { CognitiveServicesEndpoint = new Uri(cognitiveServicesEndpoint) }
                    };

                    chatHistoryCache[contextId] = new List<ChatMessage>();
                    AnswerCallResult answerCallResult = await callAutomationClient.AnswerCallAsync(options);

                    return Results.Ok();
                }
            }
        }
    }
    return Results.Ok();
});

//Handle call back event such as recognize the speech of the customer, or call connected. These events are sent by the Event grid you need to configure in the ACS resource.
app.MapPost("/api/callbacks/{contextId}", async (CloudEvent[] cloudEvents, ILogger<Program> logger, [FromRoute] string contextId,
    [Required] string callerId) => {
        foreach (var cloudEvent in cloudEvents)
        {
            var parsedEvent = CallAutomationEventParser.Parse(cloudEvent);
            logger.LogInformation($"{parsedEvent?.GetType().Name} parsedEvent received for call connection id: {parsedEvent?.CallConnectionId}");
            var callConnection = callAutomationClient.GetCallConnection(parsedEvent.CallConnectionId);

            if (parsedEvent is CallConnected)
            {
                chatHistoryCache[contextId].Add(new SystemChatMessage(Helper.systemPrompt));

                var connectMessage = "Hi, Welcome to Milan Cake shop. Are you calling to place an order for a cake?";
                chatHistoryCache[contextId].Add(new AssistantChatMessage(connectMessage));
                await SayAndRecognizeAsync(callConnection.GetCallMedia(), new PhoneNumberIdentifier(callerId), connectMessage);
            }

            if (parsedEvent is RecognizeFailed recognizeFailed && MediaEventReasonCode.RecognizeInitialSilenceTimedOut.Equals(parsedEvent.ResultInformation.SubCode.Value.ToString()))
            {
                Console.WriteLine($"Recognize failed: {parsedEvent.ResultInformation}");
                var noResponse = "I'm sorry, I didn't hear anything. Are you still with me?";
                await SayAndRecognizeAsync(callConnection.GetCallMedia(), new PhoneNumberIdentifier(callerId), noResponse);
                chatHistoryCache[contextId].Add(new AssistantChatMessage(noResponse));
            }

            // This event is generated when the speech is recorded by call automation service. In other words, when the user on the other end of the line has completed their sentence
            if (parsedEvent is RecognizeCompleted recogEvent
                && recogEvent.RecognizeResult is SpeechResult speech_result)
            {
                chatHistoryCache[contextId].Add(new UserChatMessage(speech_result.Speech));
                chatHistoryCache[contextId].Add(new UserChatMessage (Helper.reminderprompt));

                var function = await SelectOpenAIFuntion(speech_result.Speech);

                if (function != null)
                {
                    kernel.Plugins.TryGetFunctionAndArguments(function, out KernelFunction? kernelFunction,
                        out KernelArguments? kernelArguments);

                    // adds customer phone number to arguments
                    kernelArguments?.TryAdd("customerPhoneNumber", callerId);

                    var result = await kernel.InvokeAsync(kernelFunction!, kernelArguments);
                }

                // calling Azure Open AI to get a response for the user based on the conversation history, knowledgebase and the system prompt
                StringBuilder gptBuffer = new();

                await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(chatHistoryCache[contextId], options))
                {
                    var message = update.ContentUpdate;
                    foreach (var item in message)
                    {
                        if (string.IsNullOrEmpty(item.Text))
                        {
                            continue;
                        }

                        gptBuffer.Append(item.Text); 
                        if (sentenceSaperators.Any(item.Text.Contains))
                        {
                            var sentence = Regex.Replace(gptBuffer.ToString().Trim(), @"\[doc\d+\]", string.Empty);
                            if (!string.IsNullOrEmpty(sentence))
                            {
                                chatHistoryCache[contextId].Add(new AssistantChatMessage(sentence));
                                await SayAsync(callConnection.GetCallMedia(), new PhoneNumberIdentifier(callerId), sentence);
                                Console.WriteLine($"\t > streamed: '{sentence}'");
                                gptBuffer.Clear();
                            }
                        }
                    }
                }
                await SayAndRecognizeAsync(callConnection.GetCallMedia(), new PhoneNumberIdentifier(callerId), ".");
            }
        }
    });



app.Run();

TextSource CreateTextSource(string response) => new TextSource(response) { VoiceName = speechVoiceName };

// Convert the message to speech and play on the connected call
async Task SayAsync(CallMedia callConnectionMedia, PhoneNumberIdentifier phoneId, string response)
{
    var responseTextSource = CreateTextSource(response);
    var recognize_result = await callConnectionMedia.PlayToAllAsync(new PlayToAllOptions([responseTextSource]));
}

async Task SayAndRecognizeAsync(CallMedia callConnectionMedia, PhoneNumberIdentifier phoneId, string response)
{
    // creates the 
    var responseTextSource = CreateTextSource(response);

    var recognizeOptions =
        new CallMediaRecognizeSpeechOptions(
            targetParticipant: CommunicationIdentifier.FromRawId(phoneId.RawId))
        {
            Prompt = responseTextSource,
            EndSilenceTimeout = TimeSpan.FromMilliseconds(500)
        };

    var recognize_result = await callConnectionMedia.StartRecognizingAsync(recognizeOptions);
}

async Task<OpenAIFunctionToolCall?> SelectOpenAIFuntion(string prompt)
{
    var chatCompletionService = new AzureOpenAIChatCompletionService(aiDeploymentName, _aiClient!);
    var result = await chatCompletionService.GetChatMessageContentAsync(new ChatHistory(prompt),
        new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
            Temperature = 0
        }, kernel);
    var functionCall = ((OpenAIChatMessageContent)result).GetOpenAIFunctionToolCalls().FirstOrDefault();

    return functionCall;
}