import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { Chat } from './features/chat/chat';
import { MyProfile } from './features/my-profile/my-profile';

export const routes: Routes = [
    {path: "", component: Login, canActivate: [GuestGuard]},
    {path: "home", component: Home, children: [{path: 'chat/:id', component: Chat}, {path: 'my-profile', component: MyProfile}], canActivate: [AuthGuard]},
    {path: 'register', component: Register, canActivate: [GuestGuard]},
    { path: '', redirectTo: 'home', pathMatch: 'full' }
];
