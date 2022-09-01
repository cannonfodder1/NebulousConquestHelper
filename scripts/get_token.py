import requests
import json

# This tool allows for retrieving a bearer token required by some of the other tools.
# usage 'python get_token.py'
# will write out a text file with the bearer token for the included app_id and secret
# NOTE: Adjust the scope as needed. The included scope here is good enough for the other scripts
# *** requires a proper 'config.json' created from the 'config.example.json'

url = 'https://discord.com/api/v10'
app_id = ''
app_secret = ''

try:
    f = open("config.json")
    data = json.loads(f.read())
    app_id = data["app_id"]
    app_secret = data["app_secret"]
except Exception as ex:
    print("Failed to open config file %s" %ex)
    exit()

data = {
    'grant_type': 'client_credentials',
    'scope': 'identify connections applications.commands applications.commands.update'
}
headers = {
    'Content-Type': 'application/x-www-form-urlencoded'
}
r = requests.post('%s/oauth2/token' % url, data=data, headers=headers, auth=(app_id, app_secret))
r.raise_for_status()
print( r.json())

try:
    f = open("bearer_token.txt", "w")
    f.write(json.dumps(r.json(), indent=4))
    f.close
except:
    print("Unable to save result for some reason. Result: \n%s" % r.text)