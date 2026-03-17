import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { Register } from './features/register/register';

export const routes: Routes = [
    {path: "", component: Login},
    {path: "home", component: Home},
    {path: 'register', component: Register}
];
