# [IndieAnalytics Unity Client](https://indie-analytics.com/)

## Installation

1. Add the package using Unity's package manager by pasting the git link

`Window > Package Manager > + (plus sign at the top left) > Add package from git URL`

   Paste the git link url `https://github.com/papadp/indieanalytics_unity_client.git`

2. Add the `IndieAnalytics.asmdef` to your assembly definition (if you have an `.asmdef` file).

3. Drag the `IndieAnalytics` prefab into your scene.

4. Get an API key from https://indie-analytics.com/
   
   Paste your API key in the `Client_key` field of the prefab.
   
   
## Usage

### Send a progression event
```
IndieAnalytics indie_analytics = IndieAnalytics.GetObject();
indie_analytics.SendProgressionEvent("Level1");
```

## Features

Automatic:

- Retention
- Engagement
- Scene loading

User implemented:

- Progression
