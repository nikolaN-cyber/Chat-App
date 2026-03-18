import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { ConversationDetails, ConversationResponse } from "../models/conversation";

@Injectable({
    providedIn: 'root'
})
export class ConversationService{
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Conversation`;

    getUserConversations(){
        return this.http.get<ConversationResponse[]>(`${this.apiUrl}/get-all-user-conversations`);
    }

    getPrivateChat(targetId: number){
        return this.http.post<ConversationDetails>(`${this.apiUrl}/get-or-create-private/${targetId}`, {});
    }
}