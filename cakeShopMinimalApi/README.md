# Cake Shop Phone Order Sample

This project is a minimal API for a cake shop phone assistant that helps customers place orders for delicious cakes on a PSTN call. It utilizes various Azure services, including Azure OpenAI, Azure Communication Services, and Azure Cognitive Services, to provide an interactive experience for customers. With Azure OpenAI, the phone assistant can generate natural language responses and provide personalized recommendations. Azure Communication Services enables the integration of PSTN calling capabilities, allowing customers to place orders over the phone. Azure Cognitive Services powers the AI search functionality, making it easy for customers to find their favorite cake flavors and designs. 

## How to call the cake shop to place an order
You'll have to either set up the sample locally or deploy it to be able to test it out. Once you follow the steps below and have the app running, call on the ACS phone number configured in your resource from your phone. The app will pick it up and start talking to you. Go on - talk to the assistant to place an order for your favourite cake. Enjoy!

## Sample conversation flow
Bot: "Hello, welcome to Milan cake shop. Are you calling to place an order?"
Caller: "Yes"
Bot: "Great! Do you have a flavor in mind, or can I help you find something fruity, chocolaty, or maybe something else?"
Caller: "I’m thinking of something fruity."
Bot: "We have Berry Blast"
Caller: "Nice, tell me more about Berry Blast"
Bot: " It has velvety berry cream with chunks of fresh fruit. What size would you like? We have 6-inch and 8-inch options for $40 and $50 respectively."
Caller: "I'll take the 6-inch."
Bot: "Sure, what will be your pickup date?"
Caller: "5th September."
Bot: "Excellent! I am placing your order for a 6-inch Mango Pistachio cake for 5th September. Can you confirm if the details are correct?"
Caller: "Yes."
Bot: "Awesome, your order is now in our system. Please make a payment on our website to confirm your order. Goodbye for now."

Bot: "Hello, welcome to Milan cake shop. Are you calling to place an order?"
Caller: "Yes"
Bot: "Great! Do you have a flavor in mind, or can I help you find something fruity, chocolaty, or maybe something else?"
Caller: "what are your seasonal flavors?"
Bot: "We currently have two seasonal flavors: Autumn Spice and Summer Berry Burst. Do either of those sound good to you?"
Caller: "What’s in the Autumn Spice?"
Bot: "Autumn Spice is a warm and cozy spiced cake with hints of cinnamon, nutmeg, and cloves, topped with a maple cream cheese frosting. Would you like to go with that one?"
Caller: "That sounds delicious! I’ll take it."
Bot: "Great choice! What size would you like? We have 6-inch and 8-inch options for $35 and $45 respectively."
Caller: "I’ll go with the 8-inch."
Bot: "Perfect! And what will be your pickup date?"
Caller: "September 10th."
Bot: "Excellent! I’m placing your order for an 8-inch Autumn Spice cake for September 10th. Can you confirm if the details are correct?"
Caller: "Yes, that’s correct."
Bot: "Awesome, your order is now in our system. Please make a payment on our website to confirm your order. Goodbye for now!"

##Running the App

## Local Setup

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/cakeshopminimalapi.git
    cd cakeshopminimalapi
    ```

2. Set up the environment variables:

Before running the application, make sure to set up the necessary environment variables. These variables are essential for the proper functioning of the project and allow you to securely store sensitive information.

To set up the environment variables, you can use the `dotnet user-secrets` command. This command allows you to store and retrieve sensitive data securely during development without exposing it in your codebase or version control system.

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

4. The application will open up on localhost on the port mentioned in launchSettings.json. `http://localhost:port/swagger`.

5. Register your local server into event grid
This step is required to receive the notifications for incoming call from ACS.

Once you get your devtunnel url(run locally) or web app url running locally (step 4):
1. click your Azure Communciation Resource, on the right panel, click Events.
2. click "+ Event Subscription" on the top
3. In event types - Filter to Event Types, choose incoming call
4. In Endpoint details, choose Webhook
5. Make sure your local server is running before go next step, Endpoint setup needs to send signals to your server
6. Input webhook url as Endpoint, https://[your server url]/api/event
7. Save the config


Steps to create devtunnel
If you are using Visual studio, you can create a devtunnel for your project in the IDE. Follow steps here.

## Deployment on Azure

To deploy this application on Azure, follow these steps:

1. **Create an Azure App Service**:
    - Navigate to the Azure portal and create a new App Service.
    - Choose the appropriate subscription, resource group, and configure the app name, runtime stack (e.g., .NET 6), and region.

2. **Configure Deployment**:
    - In the App Service, go to the "Deployment Center" and set up continuous deployment using your preferred method (e.g., GitHub, Azure Repos, or local Git).
    - Connect your repository and branch to the App Service.

    3. **Set Up Azure Key Vault**:
    - Create an Azure Key Vault in the Azure portal.
    - Add the necessary secrets to the Key Vault. These should match the variables used in your `secrets.json` file:
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
    - In the Key Vault, go to "Access policies" and grant the App Service managed identity access to the Key Vault.
    - Ensure the App Service has "Get" permissions for secrets.

5. **Reference Key Vault Secrets in App Service**:
    - In the App Service, go to "Configuration" and add the necessary environment variables under the "Application settings" section.
    - Use the following format to reference Key Vault secrets:
        ```plaintext
        @Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/<secret-name>)
        ```
    - For example:
        - [`OPENAI_DEPLOYMENT_NAME`]: `@Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/OPENAI_DEPLOYMENT_NAME)`
        - [`OPENAI_KEY`]: `@Microsoft.KeyVault(SecretUri=https://<your-key-vault-name>.vault.azure.net/secrets/OPENAI_KEY)`
        - And so on for the other secrets.

3. **Set Environment Variables**:
    - In the App Service, go to "Configuration" and add the necessary environment variables under the "Application settings" section. These should match the variables used in your `secrets.json` file:
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

4. **Deploy the Application**:
    - Once the deployment configuration is set, trigger a deployment from your repository.
    - Monitor the deployment process in the "Deployment Center" to ensure it completes successfully.

5. **Register the Callback URI**:
    - After deployment, update the `CALLBACK_URI` environment variable with the live URL of your deployed application.
    - Register this URL in the Azure Communication Services Event Grid as described in the installation steps for local run.

6. **Access the Application**:
- Once deployed, you can access your application via the Azure App Service URL. The API documentation should be available at `https://<your-app-service-name>.azurewebsites.net/swagger`.


**Note:**

This sample is intended to demonstrate how to integrate PSTN calling functionality into your AI applications using Azure Communication Services (ACS) alongside Azure OpenAI and Azure AI Search. While testing this sample, you’ll find the assistant helpful for most parts of the conversation and capable of placing and taking orders. However, for some more complex interactions, the AI assistant may be less than perfect. That's because the focus of this sample is primarily to show how how straightforward it can be to integrate ACS PSTN calling into your RAG (Retrieve, Augment, and Generate) or AI applications. For best practices in writing prompts or handling chat completions, please refer to the OpenAI or Azure OpenAI documentation.

## Contributing

Please fork the repository and submit pull requests for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.












































    "OPENAI_DEPLOYMENT_NAME": "your_openai_deployment_name",
    "OPENAI_KEY": "your_openai_key",
    "OPENAI_ENDPOINT": "your_openai_endpoint",
    "AI_SEARCH_KEY": "your_ai_search_key",
    "AI_SEARCH_ENDPOINT": "your_ai_search_endpoint",
    "AI_SEARCH_INDEXNAME": "your_ai_search_indexname",
    "ACS_CONNECTION_STRING": "your_acs_connection_string",
    "AZURE_COG_SERVICES_ENDPOINT": "your_azure_cog_services_endpoint",
    "CALLBACK_URI": "your_callback_uri",
    "ACS_PHONE_NUMBER": "your_acs_phone_number"
}
```

Make sure to replace the placeholder values with the actual credentials and endpoints obtained from your Azure resources.

Remember to keep these environment variables secure and avoid committing them to version control systems to prevent unauthorized access to your resources.

    - Create a `secrets.json` file or set environment variables as follows:
        ```json
        {
            "OPENAI_DEPLOYMENT_NAME": "your_openai_deployment_name",
            "OPENAI_KEY": "your_openai_key",
            "OPENAI_ENDPOINT": "your_openai_endpoint",
            "AI_SEARCH_KEY": "your_ai_search_key",
            "AI_SEARCH_ENDPOINT": "your_ai_search_endpoint",
            "AI_SEARCH_INDEXNAME": "your_ai_search_indexname",
            "ACS_CONNECTION_STRING": "your_acs_connection_string",
            "AZURE_COG_SERVICES_ENDPOINT": "your_azure_cog_services_endpoint",
            "CALLBACK_URI": "your_callback_uri",
            "ACS_PHONE_NUMBER": "your_acs_phone_number"
        }
        ```

## Usage

1. Build and run the application:
    ```bash
    dotnet build
    dotnet run
    ```

2. Access the API documentation at `http://localhost:5000/swagger`.



## Contributing

Please fork the repository and submit pull requests for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.