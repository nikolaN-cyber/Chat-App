import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { AddUsersRequest, ConversationDetails, ConversationResponse, CreateConversationData, ParticipantNames } from "../models/conversation";
import { map } from "rxjs";
import { ApiResponse } from "../models/api-response"; // Proveri putanju do interfejsa

@Injectable({
    providedIn: 'root'
})
export class ConversationService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Conversation`;

    getUserConversations() {
        return this.http.get<ApiResponse<ConversationResponse[]>>(`${this.apiUrl}/get-all-user-conversations`).pipe(
            map(res => res.data ?? [])
        );
    }

    getConversation(conversationId: number) {
        return this.http.post<ApiResponse<ConversationDetails>>(`${this.apiUrl}/get-private/${conversationId}`, {}).pipe(
            map(res => res.data)
        );
    }

    createConversation(data: CreateConversationData) {
        return this.http.post<ApiResponse<ConversationResponse>>(`${this.apiUrl}/create`, data).pipe(
            map(res => res.data)
        );
    }

    deleteConversation(id: number) {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/delete/${id}`).pipe(
            map(res => res.success)
        );
    }

    removeUser(userId: number, conversationId: number) {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/remove/${conversationId}/${userId}`).pipe(
            map(res => res.success)
        );
    }

    addUsers(data: AddUsersRequest) {
        return this.http.post<ApiResponse<ParticipantNames[]>>(`${this.apiUrl}/add-users`, data).pipe(
            map(res => res.data ?? [])
        );
    }
}