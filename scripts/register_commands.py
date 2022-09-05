import requests
import json

# This tool allows for registering a set of commands
# usage 'python register_commands.py'
# *** requires a proper 'config.json' created from the 'config.example.json'
# *** requires the 'bot_token' field in the config

app_id = ''
guild_id = ''
bot_token = ''
events = []

try:
    f = open("config.json")
    data = json.loads(f.read())
    bot_token = data["bot_token"]
    app_id = data["app_id"]
    guild_id = data["guild_id"]

    f = open("events.json")
    data = json.loads(f.read())
    events = data["events"]
except Exception as ex:
    print("Failed to open file %s" %ex)
    exit()

url = "https://discord.com/api/v10/applications/%s/guilds/%s/commands" %(app_id, guild_id)

headers = {
    "Authorization": "Bot %s" %(bot_token)
}
for event in events:
    r = requests.post(url, headers=headers, json=event)
    r.raise_for_status()
    print( json.dumps(r.json(), indent=4) )