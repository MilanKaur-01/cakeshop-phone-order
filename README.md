# Cake Shop Phone Order Sample

This project is a minimal API for a cake shop phone assistant that helps customers place orders for delicious cakes on a PSTN call. It uses a number of Azure services, including Azure OpenAI, Azure Communication Services, and Azure Cognitive Services, to provide an interactive experience for customers. With Azure OpenAI, the phone assistant can generate natural language responses and provide personalized recommendations. Azure Communication Services enables the integration of PSTN calling capabilities, allowing customers to place orders over the phone. Azure Cognitive Services powers the AI search functionality, making it easy for customers to find their favorite cake flavors and designs. 

## How to call the cake shop to place an order

You'll have to either set up the sample locally or deploy it to be able to test it out. Once you complete the steps in this ReadMe and have the app running, use your phone to call the Azure Communication Services phone number configured in your resource. The app answers the call and starts talking to you. Go ahead and talk to the assistant to place an order for your favorite cake. Enjoy!

## Sample conversation flow

Bot: "Hello, welcome to Milan cake shop. Are you calling to place an order?" <br />
Caller: "Yes" <br />
Bot: "Great! Do you have a flavor in mind, or can I help you find something fruity, chocolatey, or maybe something else?" <br />
Caller: "I’m thinking of something fruity." <br />
Bot: "We have Berry Blast" <br />
Caller: "Nice, tell me more about Berry Blast" <br />
Bot: " It has velvety berry cream with chunks of fresh fruit. What size would you like? We have 6-inch and 8-inch options for $40 and $50 respectively." <br />
Caller: "I'll take the 6-inch." <br />
Bot: "Sure, what will be your pickup date?" <br />
Caller: "5th September." <br />
Bot: "Excellent! I am placing your order for a 6-inch Mango Pistachio cake for 5th September. Can you confirm if the details are correct?" <br />
Caller: "Yes." <br />
Bot: "Awesome, your order is now in our system. Please make a payment on our website to confirm your order. Goodbye for now." <br />
<br />
Bot: "Hello, welcome to Milan cake shop. Are you calling to place an order?" <br />
Caller: "Yes" <br />
Bot: "Great! Do you have a flavor in mind, or can I help you find something fruity, chocolaty, or maybe something else?" <br />
Caller: "what are your seasonal flavors?" <br />
Bot: "We currently have two seasonal flavors: Autumn Spice and Summer Berry Burst. Do either of those sound good to you?" <br />
Caller: "What’s in the Autumn Spice?" <br />
Bot: "Autumn Spice is a warm and cozy spiced cake with hints of cinnamon, nutmeg, and cloves, topped with a maple cream cheese frosting. Would you like to go with that one?" <br />
Caller: "That sounds delicious! I’ll take it." <br />
Bot: "Great choice! What size would you like? We have 6-inch and 8-inch options for $35 and $45 respectively." <br />
Caller: "I’ll go with the 8-inch." <br />
Bot: "Perfect! And what will be your pickup date?" <br />
Caller: "September 10th." <br />
Bot: "Excellent! I’m placing your order for an 8-inch Autumn Spice cake for September 10th. Can you confirm if the details are correct?" <br />
Caller: "Yes, that’s correct." <br />
Bot: "Awesome, your order is now in our system. Please make a payment on our website to confirm your order. Goodbye for now!"

## Running the App

### Prerequisites
Let's create all the necessary resources needed to run this project. Note the keys and the connection strings. You need them for the configuration.

1. [Create a free Azure account](https://azure.microsoft.com/free/) if you do not have one.
2. [Create an Azure Communication Services resource](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource).
3. [Get an Azure Communication Services phone number](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/telephony/get-phone-number).
4. [Create an Azure AI multiservice](https://learn.microsoft.com/en-us/azure/ai-services/multi-service-resource). You need this for speech-to-text and text-to-speech.
5. [Connect your Azure Communication Services resource with your Azure AI multiservice resource](https://learn.microsoft.com/en-us/azure/communication-services/concepts/call-automation/azure-communication-services-azure-cognitive-services-integration).
6. [Create and deploy an Azure Open AI resource](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource). This demo uses [gpt-35-turbo](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart).
7. [Create an Azure AI search resource](https://learn.microsoft.com/en-us/azure/search/cognitive-search-quickstart-blob) and follow the steps to upload `cakeshop.doc` as the sample file. This document is available present in cakeShopMinimalApi/docs/ location.
   

### Local Setup

1. Clone the repository:

    ```bash
    git clone https://github.com/yourusername/cakeshopminimalapi.git
    cd cakeshopminimalapi
    ```

2. Set up the environment variables:

Before running the application, make sure to set up the necessary environment variables. These variables are essential for the proper function of the project and enable you to securely store sensitive information.

To set up the environment variables, you can use the `dotnet user-secrets` command. This command enables you to store and retrieve sensitive data securely during development without exposing it in your codebase or version control system.

To set up the environment variables using `dotnet user-secrets`, follow these steps:

- Open a command prompt or terminal in the project directory.
- Run the following command to set the environment variables:
    ```bash
    dotnet user-secrets set "OPENAI_DEPLOYMENT_NAME" "your_openai_deployment_name"
    dotnet user-secrets set "OPENAI_KEY" "your_openai_key"
    dotnet user-secrets set "OPENAI_ENDPOINT" "your_openai_endpoint"
    dotnet user-secrets set "AI_SEARCH_KEY" "your_ai_search_key"
    dotnet user-secrets set "AI_SEARCH_ENDPOINT" "your_ai_search_endpoint"
    dotnet user-secrets set "AI_SEARCH_INDEXNAME" "your_ai_search_indexname"
    dotnet user-secrets set "ACS_CONNECTION_STRING" "your_acs_connection_string"
    dotnet user-secrets set "AZURE_COG_SERVICES_ENDPOINT" "your_azure_cog_services_endpoint"
    dotnet user-secrets set "CALLBACK_URI" "your_callback_uri"
    dotnet user-secrets set "ACS_PHONE_NUMBER" "your_acs_phone_number"
    ```

Make sure to replace the placeholder values with the actual credentials and endpoints obtained from your Azure resources.

Remember to keep these environment variables secure and avoid committing them to version control systems to prevent unauthorized access to your resources.

3. Build and run the application using CLI or run directly through your IDE.

    ```bash
    dotnet build
    dotnet run
    ```

4. The application runs on localhost on the port mentioned in `launchSettings.json`. `http://localhost:port/swagger`.

5. Register your local server into Azure Event Grid.

This step is required for the app to receive notifications for incoming calls from Azure Communication Services.

Once you get your devtunnel URL (run locally) or web app URL running locally (step 4):
1. From the right panel, select your Azure Communication Resource, then click **Events**.
2. At the top, click **+ Event Subscription**.
3. In Event Types, filter to Event Types and choose **Incoming call**.
4. In Endpoint details, choose **Webhook**.
5. Make sure your local server is running before continuing to the next step. Endpoint setup needs to send signals to your server.
6. Input webhook URL as Endpoint, `https://[your server URL]/api/event`.
7. Save the configuration.


### Steps to create devtunnel

Option 1 : If you are using Visual Studio, you can create a devtunnel for your project in the IDE. [Follow these steps](https://learn.microsoft.com/en-us/aspnet/core/test/dev-tunnels?view=aspnetcore-8.0#create-a-tunnel).

Option 2: You can create a dev tunnel by running these commands from your command prompt as well.

```bash
devtunnel create --allow-anonymous
devtunnel port create -p 8080
devtunnel host
```

### Deployment on Azure

To deploy this application on Azure, follow these steps:

1. **Create an Azure App Service**:
    - Navigate to the Azure portal and create a new App Service.
    - Choose the appropriate subscription, resource group, and configure the app name, runtime stack (such as .NET 6), and region.

2. **Configure Deployment**:
    - In the App Service, go to the **Deployment Center** and set up continuous deployment using your preferred method (such as GitHub, Azure Repos, or local Git).
    - Connect your repository and branch to the App Service.

3. **Set Up Azure Key Vault**:
    - Create an Azure Key Vault in the Azure portal.
    - Add the necessary secrets to the Key Vault. These must match the variables used in your `secrets.json` file:
        - `OPENAI_DEPLOYMENT_NAME`
        - `OPENAI_KEY`
        - `OPENAI_ENDPOINT`
        - `AI_SEARCH_KEY`
        - `AI_SEARCH_ENDPOINT`
        - `AI_SEARCH_INDEXNAME`
        - `ACS_CONNECTION_STRING`
        - `AZURE_COG_SERVICES_ENDPOINT`
        - `CALLBACK_URI`
        - `ACS_PHONE_NUMBER`

4. **Grant Access to Key Vault**:
    - In the Key Vault, go to **Access policies** and grant the App Service managed identity access to the Key Vault.
    - Ensure the App Service has `Get` permission for secrets.

5. **Reference Key Vault Secrets in App Service**:
    - In the App Service, go to **Configuration** and add the necessary environment variables under the **Application settings** section.
    - Use the following format to reference Key Vault secrets:
        ```plaintext
        @Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/<secret-name>)
        ```
    - For example:
        - \[`OPENAI_DEPLOYMENT_NAME`\]: `@Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/OPENAI_DEPLOYMENT_NAME)`
        - \[`OPENAI_KEY`\]: `@Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/OPENAI_KEY)`
        - And so on for the other secrets.

6. **Set Environment Variables**:
    - In the App Service, go to **Configuration** and add the necessary environment variables under the **Application settings** section. These must match the variables used in your `secrets.json` file:
        - `OPENAI_DEPLOYMENT_NAME`
        - `OPENAI_KEY`
        - `OPENAI_ENDPOINT`
        - `AI_SEARCH_KEY`
        - `AI_SEARCH_ENDPOINT`
        - `AI_SEARCH_INDEXNAME`
        - `ACS_CONNECTION_STRING`
        - `AZURE_COG_SERVICES_ENDPOINT`
        - `CALLBACK_URI`
        - `ACS_PHONE_NUMBER`

7. **Deploy the Application**:
    - Once the deployment configuration is set, trigger a deployment from your repository.
    - Monitor the deployment process in the **Deployment Center** to ensure it completes successfully.

8. **Register the Callback URI**:
    - After deployment, update the `CALLBACK_URI` environment variable with the live URL of your deployed application.
    - Register this URL in the Azure Communication Services Event Grid as described in the installation steps for local run.

9. **Access the Application**:
- Once deployed, you can access your application via the Azure App Service URL. The API documentation should be available at `https://<your-app-service-name>.azurewebsites.net/swagger`.

**Note:**

This sample demonstrates how to integrate PSTN calling functions into your AI applications using Azure Communication Services alongside Azure OpenAI and Azure AI Search. While testing this sample, you’ll find the assistant helpful for most parts of the conversation and capable of placing and taking orders. However, for more complex interactions, the AI assistant may be less than perfect. That's because the focus of this sample is primarily to show how straightforward it can be to integrate Azure Communication Services PSTN calling into your RAG (Retrieve, Augment, and Generate) or AI applications. For best practices in writing prompts or handling chat completions, see the [OpenAI](https://platform.openai.com/docs/concepts) or [Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/) documentation.

## Contributing

Fork the repository and submit pull requests for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.
