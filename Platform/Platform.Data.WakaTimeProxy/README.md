# Platform.Data.WakaTimeProxy

A local proxy server for WakaTime that stores heartbeat data locally as soon as it's received, solving the slow export problem from WakaTime.

## Features

- **Immediate Local Storage**: Stores all WakaTime heartbeats locally in real-time
- **Fast Data Access**: Heartbeats are saved to JSON Lines format for quick retrieval
- **Optional Forwarding**: Can optionally forward heartbeats to the official WakaTime API
- **WakaTime API Compatible**: Implements the same API endpoints as WakaTime

## How It Works

The proxy intercepts heartbeat data from WakaTime plugins and:
1. **Immediately stores** them locally (priority)
2. Optionally forwards them to WakaTime API in the background

This ensures your data is captured locally without waiting for WakaTime's slow export.

## Installation

1. Build the project:
```bash
cd Platform/Platform.Data.WakaTimeProxy
dotnet build
```

2. Run the proxy server:
```bash
dotnet run
```

The server will start on `http://localhost:52595` (WakaTime's default API endpoint port).

## Configuration

Edit `appsettings.json`:

```json
{
  "WakaTime": {
    "ForwardingEnabled": false
  }
}
```

- Set `ForwardingEnabled` to `true` if you want heartbeats forwarded to WakaTime API
- Set to `false` to only store locally

## Configuring WakaTime Plugins

Point your WakaTime plugin to use the local proxy instead of the official API:

1. Set the WakaTime API URL environment variable:
```bash
export WAKATIME_API_URL=http://localhost:52595/api
```

Or configure in your plugin settings to use `http://localhost:52595/api` as the API endpoint.

## Data Storage

Heartbeats are stored in JSON Lines format at:
- **Windows**: `%LocalAppData%\WakaTimeProxy\heartbeats\`
- **Linux/Mac**: `~/.local/share/WakaTimeProxy/heartbeats/`

Files are organized by date: `YYYY-MM-DD.jsonl`

Each line contains a JSON object representing one heartbeat.

## API Endpoints

The proxy implements the following WakaTime API endpoints:

- `POST /api/v1/users/{user}/heartbeats` - Store a single heartbeat
- `POST /api/v1/users/{user}/heartbeats.bulk` - Store multiple heartbeats (up to 25)
- `GET /api/v1/users/{user}/heartbeats?date=YYYY-MM-DD` - Retrieve heartbeats for a specific date

## Benefits

- **No More Slow Exports**: Data is immediately available locally
- **Data Ownership**: Your coding activity is stored on your machine
- **Offline Support**: Works without internet connection
- **Fast Queries**: Direct file access for quick data retrieval

## Troubleshooting

If the proxy isn't receiving data:
1. Verify the WakaTime plugin is configured to use the local endpoint
2. Check that the proxy is running on port 52595
3. Ensure firewall isn't blocking the connection

## License

Same as LinksPlatform project.
