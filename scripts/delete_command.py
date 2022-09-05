import requests
import sys
import json
from os.path import exists

# This tool allows for deleting a command by <id>. Use 'get_commands.py' to retrieve a list of commands
# usage 'python delete_command.py <command>'
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

com_id = sys.argv[1]

print("Deleting command %s" % com_id)

url = "https://discord.com/api/v10/applications/%s/guilds/%s/commands/%s" %(app_id, guild_id, com_id)

headers = {
    "Authorization": "Bearer %s" %(bearer_token)
}

r = requests.delete(url, headers=headers)

r.raise_for_status()
try:
    print(r.json())
except:
    print(r.text)