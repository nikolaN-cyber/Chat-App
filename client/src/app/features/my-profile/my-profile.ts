import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { authStore } from '../../shared/store/auth.store';
import { environment } from '../../../environments/environment.development';
import { userStatusStore } from '../../shared/store/user-status.store';
import { MatSelectModule } from '@angular/material/select';
import { UserStatusRequest } from '../../core/models/user';
import { MatMenuModule } from '@angular/material/menu';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-my-profile',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatMenuModule,
    DatePipe
  ],
  templateUrl: './my-profile.html',
  styleUrl: './my-profile.css',
})
export class MyProfile {
  readonly authStore = inject(authStore);
  readonly userStatusStore = inject(userStatusStore);

  getEndOfWorkDay() {
    const date = new Date();
    date.setHours(17, 0, 0, 0);
    return date.toISOString();
  }

  getEndOfDay() {
    const date = new Date();
    date.setHours(23, 59, 59, 999);
    return date.toISOString();
  }

  statusOptions: UserStatusRequest[] = [
    { emoji: 'palm-tree', status: 'onvacation', expiresAt: this.getEndOfDay() },
    { emoji: 'house', status: 'workingremotely', expiresAt: this.getEndOfWorkDay() }
  ]

  user = this.authStore.currentUser;

  public imageBaseUrl = environment.imageBaseUrl;
  
  constructor() {
    this.userStatusStore.getStatus();
  }

  onStatusChange(option: any) {
    let expiry: string;
    if (option.status === 'onvacation') {
      expiry = this.getEndOfDay();
    } else {
      expiry = this.getEndOfWorkDay();
    }
    const request: UserStatusRequest = {
      emoji: option.emoji,
      status: option.status,
      expiresAt: expiry
    };
    this.userStatusStore.updateStatus(request);
  }

  formatStatusLabel(status: string): string {
    if (status === 'onvacation') return 'On Vacation';
    if (status === 'workingremotely') return 'Working Remotely';
    return status;
  }
}
