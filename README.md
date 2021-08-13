![](https://github.com/kevinta893/puppet-bot/workflows/Build/badge.svg)

# puppet-bot
A puppeteering bot that lets you send messages to Discord with a simple chat interface.

## Bot Configuration

1. Setup your bot in the [Discord Developer Portal](https://discord.com/developers/applications)
2. Configure your bot name and avatar
3. Create an invite URL for yourself under the OAuth2 setting. Check *Scope > iot* and *Bot Permissions > Send Messages*
4. Copy the invite URL and visit the URL with any browser
5. Invite the bot to your sever

## Using the Client app

Enter a valid bot token in the included *appsettings.json* file to get started. Run the app to get started! Select a server and channel to start sending messages.

You can *optionally* specify multiple bot tokens in the list and then launch the application with a command line argument such as

`.\PuppetBotClient.exe 0`

to select the bot token you wish to use. The above example picks the first token in the list. Useful for setting up shortcut files. In Windows shortcut files, add this index to the **target**.

## Editing a message

First select a channel from the server that contains the message you want to edit. Then copy the ID of the message you want to edit from Discord and paste it into the new edit window. Click on the `Get message` button and you should see your message appear and ready to edit.