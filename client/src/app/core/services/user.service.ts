import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { Message } from "../models/message";
import { MessageResponse } from "../models/conversation";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/User`;

    sendMessage(data: Message){
        return this.http.post<MessageResponse>(`${this.apiUrl}/send-message`, data);
    }
}