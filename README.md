# Slack File Download

This tool downloads all the files uploaded to a Slack Workspace in messages
exported using the Export Data tool:

```
https://<your workspace name>.slack.com/services/export
```

## Build
1. Install [.NET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
or later.

2. Either use an IDE like Visual Studio Code to build the code, or run directly
on the terminal:
    ```
    dotnet build SlackFileDownload.sln
    ```

## Use
1. Extract the files in the zip archive from the Export Data tool.

2. Run the tool with the path to the folder containing the Slack export:
    ```
    SlackFileDownload.exe [path to Slack files]
    ```

    If the tool is in the same folder as the Slack export, specifying the path
    is not necessary.

3. The files will be downloaded into the same folder under the `files` directory.

## License

[MIT](LICENSE)
