class NotificationClient {
    instance = new signalR.HubConnectionBuilder()
        .withUrl("/notifications")
        .withAutomaticReconnect()
        .build();

    handlers = new Map();

    constructor() {
        this.instance.onreconnected(() => {
            console.log("⚡️ [NotificationClient]:: Reconnected")
        })

        this.instance.onreconnecting(() => {
            console.log("🔄 [NotificationClient]:: Reconnecting...")
        })
    }

    subscribe(event, callback) {
        const eventHandlers = this.handlers.get(event) ?? [];
        eventHandlers.push(callback);
        this.handlers.set(event, eventHandlers);
        this.instance.on(event, (notification) => {
            eventHandlers.forEach(eventHandler => {
                eventHandler(notification);
            })
        });
    }

    async connect() {
        return this.instance.start()
            .then(
                () => {
                    console.log("🛜 [NotificationClient]:: Connected");
                    alert("🛜 [NotificationClient]:: Connected");
                }, () => {
                    console.log("🚨 [NotificationClient]:: Could not connect!");
                    alert("🚨 [NotificationClient]:: Could not connect!");
                });
    }

    async disconnect() {
        await this.instance.stop();
    }

    async invoke(fn, data) {
        return await (data ? this.instance.invoke(fn, data) : this.instance.invoke(fn));
    }
}

const client = new NotificationClient();

client.connect()
    .then(() => {
        client.subscribe("ReceiveNotification", notification => {
            console.log(notification);
        })
        client.subscribe("ReceiveNotification", notification => {
            new Audio("/audio/beep.mp3").play();
        })
    });

function send() {
    client.invoke("SendNotification");
}

function ping() {
    client
        .invoke("Ping")
        .then((status) => {
            console.log(status);
            alert(status)
        }).catch(err => {
            console.log(err)
            console.log("❌ [NotificationClient]:: Disconnected]");
            alert("❌ [NotificationClient]:: Disconnected]")
            client.connect()
                .then(() => {
                    ping();
                });
        })
}

function getConnections() {
    client.invoke("GetConnections")
        .then((result) => {
            alert(result)
        })
}



