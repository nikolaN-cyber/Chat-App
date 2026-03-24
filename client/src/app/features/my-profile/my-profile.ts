import { Component, inject, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { authStore } from '../../shared/store/auth.store';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-my-profile',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './my-profile.html',
  styleUrl: './my-profile.css',
})
export class MyProfile { 
  readonly authStore = inject(authStore);
  user = this.authStore.currentUser;

  public imageBaseUrl = environment.imageBaseUrl;

  constructor() {
  }
}
