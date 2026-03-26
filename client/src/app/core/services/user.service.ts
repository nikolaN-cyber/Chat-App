import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { Message, MessageResponse } from "../models/message";
import { PhotoUpdateResponse, UserSearchResponse, UserStatusRequest, UserStatusResponse } from "../models/user";
import { map } from "rxjs";
import { ApiResponse } from "../models/api-response";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/User`;

    sendMessage(data: Message) {
        return this.http.post<ApiResponse<MessageResponse>>(`${this.apiUrl}/send-message`, data).pipe(
            map(res => res.data)
        );
    }

    searchUsersByUsername(filter: string) {
        const params = new HttpParams().set('filter', filter);
        return this.http.get<ApiResponse<UserSearchResponse[]>>(`${this.apiUrl}/search-by-username`, { params }).pipe(
            map(res => res.data ?? [])
        );
    }

    addProfilePhoto(file: File) {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<ApiResponse<PhotoUpdateResponse>>(`${this.apiUrl}/add-photo`, formData).pipe(
            map(res => res.data)
        );
    }

    updateStatus(request: UserStatusRequest) {
        return this.http.post<ApiResponse<UserStatusResponse>>(`${this.apiUrl}/update-status`, request).pipe(
            map(res => res.data)
        );
    }

    getStatus() {
        return this.http.get<ApiResponse<UserStatusResponse>>(`${this.apiUrl}/get-user-status`).pipe(
            map(res => res.data)
        );
    }
}