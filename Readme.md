# Flow Launcher OzBargain Plugin

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher) that shows [OzBargain](https://www.ozbargain.com.au/)'s feed.
<div align="center">
    <img width="1000" height="443" alt="image" src="https://github.com/user-attachments/assets/f953ad66-54f0-43e6-b851-295150dd9a2b" />
    <img width="1000" height="523" alt="image" src="https://github.com/user-attachments/assets/f558c000-1f7f-4c85-9e7f-c03566dbec97" />
</div>

### Installation
    pm install https://github.com/coweggs/Flow.Launcher.Plugin.OzBargain/releases/download/v1.1.5/Flow.Launcher.Plugin.OzBargain.zip

### Usage
By default, three options are available:
1. Search: Autocompletes `oz search `
    - Hold ctr while interacting to autocomplete `oz searchnotexpired ` instead
3. Feed: Shows preset feeds Front Page, New Deals, Freebies, and Popular Deals.
4. Refresh: Refresh all the feeds (which are cached to prevent ratelimitting.

Alternatively manually out the commands:
1. `oz <url>`: If page supports RSS, displays it's feed.
2. `oz feed`: List of preset URL's supporting RSS (not all of them)
3. `oz search <query>`: Search OzBargain.
4. `oz searchnotexpired <query>`: Search OzBargain with expired items filtered out.

Once items are loaded, interact with any item to open it in your default browser. Title, and Expiry Date/Tags, as well as an Image are shown in the results ui.
