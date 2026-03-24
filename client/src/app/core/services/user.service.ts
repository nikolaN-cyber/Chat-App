import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { Message } from "../models/message";
import { MessageResponse } from "../models/conversation";
import { PhotoUpdateResponse, UserSearchResponse } from "../models/user";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/User`;

    sendMessage(data: Message){
        return this.http.post<MessageResponse>(`${this.apiUrl}/send-message`, data);
    }

    searchUsersByUsername(filter: string){
        const params = new HttpParams().set('filter', filter);
        return this.http.get<UserSearchResponse[]>(`${this.apiUrl}/search-by-username`, {params});
    }

    addProfilePhoto(file: File){
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<PhotoUpdateResponse>(`${this.apiUrl}/add-photo`, formData);
    }
}