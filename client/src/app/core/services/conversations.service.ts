import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { ConversationResponse } from "../models/conversation";
import { HttpHeaders } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class ConversationService{
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Conversation`;

    getUserConversations(){
        return this.http.get<ConversationResponse[]>(`${this.apiUrl}/get-all-user-conversations`);
    }
}