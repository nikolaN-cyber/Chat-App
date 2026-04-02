import { Injectable, inject } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { chatStore } from "../../shared/store/chat.store";
import { authStore } from "../../shared/store/auth.store";
import { environment } from "../../../environments/environment";
import { BehaviorSubject, filter, firstValueFrom } from "rxjs";
import { conversationsStore } from "../../shared/store/conversations.store";
import { ConversationResponse } from "../models/conversation";
import { MessageResponse } from "../models/message";

@Injectable({ providedIn: 'root' })
export class ChatSignalRService {
    private hubConnection?: signalR.HubConnection;
    private chatStore = inject(chatStore);
    private authStore = inject(authStore);
    private conversationsStore = inject(conversationsStore);

    private isConnected$ = new BehaviorSubject<boolean>(false);

    public startConnection() {
        const token = this.authStore.currentUser()?.accessToken;
        if (!token || this.hubConnection?.state === signalR.HubConnectionState.Connected) {
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

        this.hubConnection.on('ReceiveMessage', (message) => {
            const activeId = this.chatStore.currentConversationId();
            if (message.conversationId === activeId) {
                this.chatStore.addMessage(message);
            } else {
                this.conversationsStore.incrementUnreadCount(message.conversationId);
            }
        });

        this.hubConnection.on('UserStatusChanged', (userId: number, isOnline: boolean) => {
            this.conversationsStore.updateUserOnlineStatus(userId, isOnline)
        });

        this.hubConnection.on('DeleteConversation', (conversationId: number, adminId: number) => {
            const myId = this.authStore.currentUser()?.id;
            if (adminId !== myId) {
                this.conversationsStore.removeConversation(conversationId)
            }
        });

        this.hubConnection.on('CreateConversation', (conversation: ConversationResponse, adminId: number) => {
            const myId = this.authStore.currentUser()?.id;
            if (adminId !== myId) {
                this.conversationsStore.addConversation(conversation)
            }
        })

        this.hubConnection.on('UserTyping', (conversationId: number, isTyping: boolean, username: string) => {
            if (this.chatStore.currentConversationId() === conversationId) {
                this.chatStore.setIsTyping(isTyping, username);
            }
        });

        this.hubConnection.on('UserAdded', (systemMessage) => {
            this.chatStore.addMessage(systemMessage);
        })

        this.hubConnection.on('UserRemoved', (systemMessage) => {
            this.chatStore.addMessage(systemMessage);
        })

        this.hubConnection
            .start()
            .then(() => {
                this.isConnected$.next(true);
            })
            .catch(err => {
                this.isConnected$.next(false);
            });
    }

    public async joinConversation(conversationId: number) {
        if (!this.hubConnection) {
            this.startConnection();
        }
        await firstValueFrom(this.isConnected$.pipe(filter(connected => connected)));
        await this.hubConnection?.invoke('JoinConversation', conversationId);
    }

    public async leaveConversation(conversationId: number) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('LeaveConversation', conversationId);
        }
    }

    public async sendTypingNotification(conversationId: number, isTyping: boolean, username: string) {
        username = this.authStore.currentUser()?.username!;
        await this.hubConnection?.invoke('UserTyping', conversationId, isTyping, username);
    }

    public stopConnection() {
        this.hubConnection?.stop().then(() => {
            this.isConnected$.next(false);
        });
    }
}