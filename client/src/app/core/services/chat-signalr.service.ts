import { Injectable, inject } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { chatStore } from "../../shared/store/chat.store";
import { authStore } from "../../shared/store/auth.store";
import { environment } from "../../../environments/environment";

@Injectable(
    { providedIn: 'root' }
)
export class ChatSignalRService {
    private hubConnection?: signalR.HubConnection;
    private chatStore = inject(chatStore);
    private authStore = inject(authStore);

    public startConnection() {
        const token = this.authStore.currentUser()?.accessToken;
        if (!token) {
            return;
        }
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(environment.signalRUrl, {
                accessTokenFactory: () => token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection
            .start()
            .then(() => console.log('SignalR: Connected!'))
            .catch(err => console.error('SignalR Error: ', err));

        this.hubConnection.on('ReceiveMessage', (message) => {
            this.chatStore.addMessage(message);
        });

        this.hubConnection.on('UserStatusChanged', (username, isOnline) => {
            console.log(`User ${username} is ${isOnline ? 'Online' : 'Offline'}`);
        });
    }

    public async joinConversation(conversationId: number) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinConversation', conversationId)
                .catch(err => console.error(err));
        }
    }

    public async leaveConversation(conversationId: number) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            try {
                await this.hubConnection.invoke('LeaveConversation', conversationId);
                console.log(`SignalR: Left conversation ${conversationId}`);
            } catch (err) {
                console.error('SignalR Error while leaving:', err);
            }
        }
    }

    public stopConnection() {
        this.hubConnection?.stop();
    }
}