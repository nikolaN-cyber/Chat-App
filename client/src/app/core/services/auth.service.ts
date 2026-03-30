import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { environment } from "../../../environments/environment";
import { LoginResponse, UserLogin, UserRegister } from "../models/user";
import { map } from "rxjs";
import { ApiResponse } from "../models/api-response";

@Injectable({
 providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Auth`;

    login(loginData: UserLogin){
        return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, loginData).pipe(
            map(response => response.data)
        );
    }

    register(registerData: UserRegister){
        return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/register`, registerData).pipe(
            map(response => response.success)
        );
    }
}