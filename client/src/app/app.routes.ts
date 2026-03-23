import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { Chat } from './features/chat/chat';
import { MyProfile } from './features/my-profile/my-profile';
import { CreateConversation } from './features/create-conversation/create-conversation';
import { ChatOptions } from './features/chat-options/chat-options';

export const routes: Routes = [
    { path: "", component: Login, canActivate: [GuestGuard] },
    {
        path: "home", component: Home, children: [
            { path: 'chat/:id', component: Chat },
            { path: 'chat/:id/options', component: ChatOptions},
            { path: 'my-profile', component: MyProfile },
            { path: 'create-conversation', component: CreateConversation },
        ], canActivate: [AuthGuard]
    },
    { path: 'register', component: Register, canActivate: [GuestGuard] },
    { path: '', redirectTo: 'home', pathMatch: 'full' }
];
