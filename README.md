# GDPRBot

## Summary
Bot for testing out Bot Framework GDPR functionality from https://blog.botframework.com/2018/04/23/general-data-protection-regulation-gdpr/.

The goal of the bot is that if you type anything other than one of these special commands, it will just echo back what you typed. Otherwise it will exercise the new GDPR methods.

Intended commands :
* get
* delete
* export
* clear user
* clear private

GDPR methods to map to :

* GetConversationsAsync()
* DeleteConversationMember()
* ExportBotStateDataAsync()
* SetUserData()
* SetPrivateConversationData()

The blog post mentions to call these not from the bot iself, but from a separate web site. The thought was to get this working in the bot at first, then move to a separate site. However, hitting hurdles just within the bot.

There is this example repo : https://github.com/Microsoft/BotFramework-Samples/tree/master/blog-samples/CSharp/BotStateExport for calling ExportBotStateDataAsync(), however it is using the deprecated state client.

Not sure the best way to get to the classes which expose these GDPR methods from outside or even within the bot. We need to get to a ```IConversations``` and ```IBotState```.

SetUserData() and SetPrivateConversationData() are deprecated so presumably the intent is to just set context.UserData etc to null?

## Config
You need to create a file called *secrets.connections.config* and set it up as follows :

```xml
<connectionStrings>
  <add name="MS_AzureStoreConnectionString" connectionString="<your connection string>" />
</connectionStrings>
```

## Progress
Currently "export" command is partially implemented for calling ExportBotStateDataAsync(). Stuck trying to work out how to get an IBotState.

 
