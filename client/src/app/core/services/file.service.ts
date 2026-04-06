import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { map } from "rxjs";
import { ApiResponse } from "../models/api-response";

@Injectable({
    providedIn: 'root'
})
export class FileService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/File`;

    uploadFile(file: File) {
        const formData = new FormData();
        formData.append('file', file);
        const extension = file.name.split('.').pop()?.toLowerCase();
        const fallbackType = extension ? `application/${extension}` : 'application/octet-stream';

        return this.http.post<ApiResponse<{ url: string }>>(
            `${this.apiUrl}/upload-chat-file`,
            formData
        ).pipe(
            map(res => ({
                url: res.data?.url,
                type: file.type || fallbackType
            }))
        );
    }
}