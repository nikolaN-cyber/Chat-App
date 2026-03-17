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
    accessToken: string;
}
