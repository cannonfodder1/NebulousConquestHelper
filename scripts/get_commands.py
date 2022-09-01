import requests
import json
from os.path import exists

# This tool allows for getting all commands for a guild
# usage 'python get_commands.py'
# *** requires a proper 'config.json' created from the 'config.example.json'
# *** requires a proper bearer_token generated with the proper scope. User 'get_token.py'

app_id = ''
guild_id = ''

try:
    f = open("config.json")
    data = json.loads(f.read())
    app_id = data["app_id"]
    guild_id = data["guild_id"]
except Exception as ex:
    print("Failed to open config file %s" %ex)
    exit()

bearer_token = ''

url = "https://discord.com/api/v10/applications/%s/guilds/%s/commands" %(app_id, guild_id)

if exists("bearer_token.txt") == False:
    print("Missing token file. Run 'get_token.py'")
    exit()

try:
    f = open("bearer_token.txt")
    token_data = json.loads(f.read())
    bearer_token = token_data["access_token"]
except Exception as ex:
    print("Failed to open token file %s" %ex)
    exit()

headers = {
    "Authorization": "Bearer %s" %(bearer_token)
}

r = requests.get(url, headers=headers)

r.raise_for_status()
print( json.dumps(r.json(), indent=4) )