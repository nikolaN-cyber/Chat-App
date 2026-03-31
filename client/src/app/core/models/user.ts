export interface UserLogin{
    username: string;
    password: string;
}

export interface UserRegister{
    username: string;
    firstName: string;
    lastName: string;
    age: number;
    email: string;
    password: string;
    confirmPassword: string;
}

export interface LoginResponse{
    id: number;
    username: string;
    firstName: string;
    lastName: string;
    age: number;
    email: string;
    photoUrl: string;
    accessToken: string;
}

export interface UserSearchResponse{
    id: number;
    username: string;
    photoUrl: string;
    status: UserStatusResponse;
}

export interface PhotoUpdateResponse {
    url: string;
}

export interface UserStatusRequest {
    emoji: string | null;
    status: string | null;
    expiresAt: string | null;
}

export interface UserStatusResponse {
    emoji: string | null;
    status: string | null;
}
