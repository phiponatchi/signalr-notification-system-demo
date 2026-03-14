# SignalR Demo

A simple real-time demo built with **ASP.NET Core 9**, **SignalR**, and a **JavaScript SignalR client**.

This project shows how a browser client can connect to a SignalR hub, call server methods, receive real-time notifications, and inspect active connection IDs.

## Features

- **Connect** to a SignalR hub from the browser
- **Disconnect** from the hub
- **Ping** the server to verify the connection
- **Send** a notification to other connected clients
- **Get connections** to inspect currently tracked connection IDs
- **Automatic reconnect** on the JavaScript client
- Simple in-memory storage for:
  - active connections
  - sent notifications
- Static frontend served directly by ASP.NET Core
- OpenAPI + Scalar enabled in development

---

## Tech Stack

### Backend
- **ASP.NET Core 9**
- **SignalR**
- **OpenAPI**
- **Scalar.AspNetCore**

### Frontend
- **HTML**
- **Vanilla JavaScript**
- **SignalR JavaScript client**

---

## Project Structure

```
signalR-demo/
├── README.md
├── signalR-demo.sln
└── signalR-demo/
    ├── Program.cs
    ├── signalR-demo.csproj
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Properties/
    │   └── launchSettings.json
    ├── hub/
    │   ├── ConnectionMapping.cs
    │   ├── IClientInterface.cs
    │   ├── Notification.cs
    │   └── NotificationHub.cs
    └── wwwroot/
        ├── audio
        │   └── beep.mp3
        ├── images
        │   ├── icon-192.png
        │   └── icon-512.png
        ├── index.html
        ├── index.js
        ├── favicon.png
        ├── manifest.json
        ├── service-worker.js
        └── sw.js
```


---

## How It Works

The application exposes a SignalR hub at: ``/notifications``

The browser creates a SignalR connection to that hub and can call hub methods such as:

- `Ping`
- `SendNotification`
- `GetConnections`

The server can also push messages back to connected clients using:

- `ReceiveNotification`

---

## Backend Overview

## `Program.cs`

The backend application:

- registers controllers
- enables OpenAPI
- enables SignalR
- serves static files from `wwwroot`
- maps the SignalR hub to `/notifications`

### Registered services

- `AddOpenApi()`
- `AddControllers()`
- `AddSignalR()`

### Endpoints

- OpenAPI endpoints in development
- static frontend assets
- SignalR hub:
  ```text
  /notifications
  ```

---

## Hub Layer

## `NotificationHub`

`NotificationHub` is the core real-time component.

It inherits from: ``Hub``
That means the server can call strongly typed client methods defined in `IClientInterface`.

### Responsibilities

- track connected clients
- remove clients when they disconnect
- create notifications
- broadcast notifications to other clients
- return stored notifications
- return active connection IDs
- support ping checks

---

## Connection Tracking

## `ConnectionMapping<T>`

This class stores SignalR connection IDs in memory.

### What it does

- adds a connection when a client connects
- removes a connection when a client disconnects
- returns all connections for a key
- returns all tracked connection IDs
- exposes a `Count`

### Implementation detail

It uses:

- `Dictionary<T, HashSet<string>?>`
- `lock` statements for thread safety

In this project, the hub stores each connection using its own `ConnectionId` as both the key and the value: `key = connectionId value = connectionId`

So `GetConnections()` effectively returns the list of currently tracked SignalR connection IDs.

---

## Client Contract

## `IClientInterface`

This interface defines methods the server can invoke on connected clients.

### Client methods

#### `ReceiveNotification(Notification notification)`
Used when the server pushes a single notification to the client.

#### `GetNotifications(List<Notification> notifications)`
Defined as part of the contract, though it is not currently invoked by the hub implementation.

---

## Notification Model

## `Notification`

Represents a notification sent through the system.

### Properties

- `Id` - unique identifier
- `IsRead` - read/unread flag
- `CreatedAt` - timestamp
- `CreatedBy` - connection ID of sender
- `Payload` - arbitrary data

In the `SendNotification()` flow, the payload contains the current connection list.

---

## Hub Methods

## Connection lifecycle

### `OnConnectedAsync()`

Called automatically when a client connects.

What it does:
- adds the current SignalR connection ID to the in-memory connection map

### `OnDisconnectedAsync(Exception? exception)`

Called automatically when a client disconnects.

What it does:
- removes the current SignalR connection ID from the in-memory connection map

---

## Real-time actions

### `Ping()`

Returns: `true`

Purpose:
- verifies the client can successfully invoke the hub
- useful for connection health checks

Frontend behavior:
- if ping fails, the client shows a disconnected message
- then it attempts to reconnect and retries the ping

---

### `SendNotification()`

Creates a new notification on the server and sends it to **all other connected clients**.

What it does:
- creates a `Notification`
- sets:
  - `Id`
  - `CreatedAt`
  - `CreatedBy` = current connection ID
  - `IsRead` = `false`
  - `Payload` = current connection IDs
- stores the notification in memory
- sends it to:
  ```csharp
  Clients.Others.ReceiveNotification(notification)
  ```

Important:
- the sender does **not** receive this notification through `Clients.Others`
- only other connected clients receive it

---

### `Broadcast(Notification notification)`

Broadcasts a provided notification object to **other clients**.

What it does:
- stores the provided notification
- sends it to other connected clients

Note:
- this method exists on the backend but is not currently used by the frontend page

---

### `GetNotifications()`

Returns all stored notifications from memory.

Important:
- notifications are stored in a static in-memory list
- data is lost when the app restarts
- this is fine for a demo, but not for production

---

### `GetConnections()`

Returns all currently tracked SignalR connection IDs.

Used by the frontend to show which clients are connected.

---

## Frontend Overview

## `wwwroot/index.html`

The page provides a minimal UI with buttons for:

- `ping`
- `connect`
- `disconnect`
- `send`
- `get connections`

It loads:

- the service worker script
- the SignalR JavaScript client from CDN
- the local `index.js`

---

## `wwwroot/index.js`

This file contains the browser-side SignalR logic.

### `NotificationClient`

A small wrapper around the SignalR JS client.

#### Connection creation

It builds a connection with: 
```javascript
new signalR.HubConnectionBuilder()
        .withUrl("/notifications")
        .withAutomaticReconnect()
        .build();
```

### Built-in reconnect behavior

The client logs:

- when reconnecting
- when reconnected

That gives basic visibility into connection recovery.

---

## Client methods

### `connect()`

Starts the SignalR connection.

Behavior:
- calls `this.instance.start()`
- shows success/failure messages using `console.log` and `alert`

Used by:
- the `connect` button
- the initial page load flow
- reconnect logic after failed ping

---

### `disconnect()`

Stops the SignalR connection.

Used by:
- the `disconnect` button

---

### `invoke(fn, data)`

Generic wrapper for calling hub methods.

Examples:
- `Ping`
- `SendNotification`
- `GetConnections`

---

### `subscribe(event, callback)`

Registers handlers for SignalR server-to-client events.

In this project, it subscribes to: `ReceiveNotification`

Two callbacks are registered:

1. log the notification to the console
2. play a beep sound

So when another client sends a notification, connected clients:
- receive the payload
- log it
- play audio feedback

Tiny demo, nice effect, very SignalR.

---

## Frontend Actions

### Connect

Button: `connect` Calls: 
```javascript
client.connect()
```

Result:
- starts the SignalR connection
- server triggers `OnConnectedAsync()`
- connection ID is added to the tracked list

---

### Disconnect

Button: `disconnect` Calls:
```javascript
client.disconnect()
```

Result:
- stops the SignalR connection
- server triggers `OnDisconnectedAsync()`
- connection ID is removed from the tracked list

---

### Ping

Button: `ping` Calls:
```javascript
client.invoke("Ping")
```

Success result:
- returns `true`
- shows alert/log output

Failure result:
- logs an error
- alerts that the client is disconnected
- tries to reconnect
- retries ping after reconnect

This is a nice little demo of resilience for a real-time client.

---

### Send

Button: `send` Calls:
```javascript
client.invoke("SendNotification")
```

Result:
- server creates a notification
- server sends it to other connected clients
- other clients receive `ReceiveNotification`
- other clients log the notification and play a beep sound

---

### Get Connections

Button: `get connections` Calls:
```javascript
client.invoke("GetConnections")
```

Result:
- server returns the currently tracked SignalR connection IDs
- frontend displays them in an alert

---

## Startup Flow

When the page loads:

1. a `NotificationClient` is created
2. it immediately calls `connect()`
3. once connected, it subscribes to `ReceiveNotification`
4. future notifications from other clients are handled in real time

---

## Running the Project

## Requirements

- **.NET 9 SDK**
- a modern browser

## Run

From the solution or project directory:
```bash
bash dotnet run --project signalR-demo
```

Based on the launch settings, the app runs on:
```
http://localhost:5125
https://localhost:7119
```

Open the frontend in your browser: [http://localhost:5125/index.html](http://localhost:5125/index.html) or [http://localhost:5125/index.html](http://localhost:5125/index.html)


---

## Testing the Demo

A simple way to test:

1. run the app
2. open the page in two browser tabs
3. click **connect** if needed
4. click **send** in one tab
5. observe the other tab:
  - receives the notification
  - logs it
  - plays the beep sound
6. click **get connections** to see active connection IDs
7. click **ping** to verify server communication
8. click **disconnect** in one tab and test again

---

## OpenAPI / API Docs

In development mode, the project enables:

- OpenAPI mapping
- Scalar API reference UI

These are configured in `Program.cs`.

Note:
- the main demo behavior is driven by SignalR rather than traditional REST endpoints

---

## Configuration

## `appsettings.json`
Contains default logging configuration.

## `appsettings.Development.json`
Contains development logging configuration.

## `launchSettings.json`
Defines development launch profiles and URLs.

Current profiles include:
- HTTP
- HTTPS

---

## Important Notes

### In-memory state
Connections and notifications are stored in static in-memory collections.

That means:

- restarting the app clears everything
- this is suitable for demos and learning
- production apps should use persistent storage and/or distributed backplanes where needed

### Notifications go to others only
`SendNotification()` uses: 
```csharp
Clients.Others.ReceiveNotification(notification)
```


So the sender does not receive its own pushed notification.

### `GetNotifications()` is available but unused by the UI
The backend can return notification history, but the current frontend does not call that method yet.

### `Broadcast(Notification notification)` exists but is unused by the UI
The frontend currently sends notifications via `SendNotification()` only.

---

## Possible Improvements

Some useful next steps if you want to evolve this demo:

- add a visible notification list in the UI
- show current connection state on screen
- render active connection IDs instead of using `alert`
- add a button for `GetNotifications()`
- add a form to call `Broadcast(Notification notification)`
- persist notifications in a database
- support users/groups instead of raw connection IDs
- improve reconnect UX
- add timestamps and better formatting in the frontend
- avoid duplicate event registration if reconnect flows are expanded

---

## Summary

This project is a compact learning demo for SignalR with:

- an **ASP.NET Core backend**
- a **typed SignalR hub**
- a **JavaScript client with automatic reconnect**
- real-time notification delivery
- connection tracking
- simple interactive browser controls

It is especially useful for understanding the basics of:

- SignalR hub methods
- client-to-server invocation
- server-to-client events
- connection lifecycle handling
- reconnect behavior in the browser

---

## License
MIT


