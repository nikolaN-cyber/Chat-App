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
import { Welcome } from './features/welcome/welcome';

export const routes: Routes = [
    { path: "", component: Login, title: 'ChatApp - Login', canActivate: [GuestGuard] },
    {
        path: "home", component: Home, children: [
            {path: '', title: 'ChatApp - Inbox', component: Welcome},
            { path: 'chat/:id', title: 'ChatApp - Inbox', component: Chat },
            { path: 'chat/:id/options', title: 'ChatApp - Options', component: ChatOptions},
            { path: 'my-profile', title: 'ChatApp - MyProfile', component: MyProfile },
            { path: 'create-conversation', title: 'ChatApp - Create Conversation', component: CreateConversation },
        ], canActivate: [AuthGuard]
    },
    { path: 'register', title: 'ChatApp - Register', component: Register, canActivate: [GuestGuard] },
    { path: '', redirectTo: 'home', pathMatch: 'full' }
];
