import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { LoginResponse, UserLogin, UserRegister } from "../models/user";

@Injectable({
 providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Auth`;

    login(loginData: UserLogin){
        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, loginData);
    }

    register(registerData: UserRegister){
        return this.http.post<boolean>(`${this.apiUrl}/register`, registerData)
    }
}