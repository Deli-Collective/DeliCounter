# ⚠️ DEPRECATION NOTICE
Unfortunately as of July 27, 2021, the mod host website which this mod manager used is now permanently offline. What this means is you will be unable to install the majority of mods which are listed. Deli Counter was my stopgap while we waited for something more and now since we are hosting mods on [Thunderstore](https://h3vr.thunderstore.io/) we have that. I highly recommend you switch to using [r2modman](https://h3vr.thunderstore.io/package/ebkr/r2modman/). I'm glad I was able to provide a tool which so many people used.

Cheers.

# Deli Counter
Deli Counter is a mod manager for games supporting BepInEx / Deli with a nice clean modern UI
![image](https://user-images.githubusercontent.com/22647801/118531246-9fe86000-b713-11eb-974c-a46337fe7231.png)

## Usage
1. Download the [latest release](https://github.com/Deli-Collective/Slicer/releases/latest/)
2. Extract the application
3. Run DeliCounter.exe

## Advanced usage
* Using standard list view multi select keys you can install multiple mods at the same time (Holding control or shift while selecting items)
* You can change the repsitory you pull mods from by entering the settings page and replacing the GitHub URL there with the URL of another. This can be very useful to test changes made on database forks before creating a PR to merge them in. Additionally, you can specify the branch by adding `/tree/{branch}` to the end of the URL. This matches the GitHub URL for that branch, so you could also select the branch in a web browser and then copy the URL from there too.

## Where do the mods come from
The mods listed in Deli Counter are pulled from a seperate dedicated repository because it keeps the git history clean and makes it easier for people to make edits and PRs. Deli Counter clones the repository locally and fetches the latest commits when it tries to update so any changes can be received immidiately. The repository can be found here: 
https://github.com/Deli-Collective/Deli-Counter-Database.

### How do I add my own mod to the database?
Follow this guide on the database repo: https://github.com/Deli-Collective/Deli-Counter-Database/blob/main/CONTRIBUTING.md
