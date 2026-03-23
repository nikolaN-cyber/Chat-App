import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { AddUsersRequest, ConversationDetails, ConversationResponse, CreateConversationData, ParticipantNames } from "../models/conversation";

@Injectable({
    providedIn: 'root'
})
export class ConversationService{
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Conversation`;

    getUserConversations(){
        return this.http.get<ConversationResponse[]>(`${this.apiUrl}/get-all-user-conversations`);
    }

    getConversation(conversationId: number){
        return this.http.post<ConversationDetails>(`${this.apiUrl}/get-private/${conversationId}`, {});
    }

    createConversation(data: CreateConversationData){
        return this.http.post<ConversationResponse>(`${this.apiUrl}/create`, data);
    }

    deleteConversation(id: number){
        return this.http.delete<boolean>(`${this.apiUrl}/delete/${id}`);
    }

    removeUser(userId: number, conversationId: number){
        return this.http.delete<boolean>(`${this.apiUrl}/remove/${conversationId}/${userId}`);
    }

    addUsers(data: AddUsersRequest){
        return this.http.post<ParticipantNames[]>(`${this.apiUrl}/add-users`, data);
    }
}