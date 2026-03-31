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
import { userStore } from '../../shared/store/user.store';

@Component({
  selector: 'app-my-profile',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatMenuModule,
  ],
  templateUrl: './my-profile.html',
  styleUrl: './my-profile.css',
})
export class MyProfile {
  readonly authStore = inject(authStore);
  readonly userStore = inject(userStore);
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
    const request: UserStatusRequest = {
      emoji: option.emoji,
      status: option.status,
      expiresAt: option.status === 'onvacation' ? this.getEndOfDay() : this.getEndOfWorkDay()
    };

    this.userStatusStore.updateStatus(request);
  }

  formatStatusLabel(status: string | undefined | null): string {
    if (!status || status === 'No status set') return 'No status active';
    if (status === 'onvacation') return 'On Vacation';
    if (status === 'workingremotely') return 'Working Remotely';
    return status;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      console.log('Fajl izabran:', file.name, file.size, file.type);

      this.userStore.updatePhoto(file);
      input.value = '';
    } else {
      console.log('Nijedan fajl nije izabran.');
    }
  }

  clearStatus() {
    const clearRequest: UserStatusRequest = {
      emoji: null,
      status: null,
      expiresAt: null
    };
    this.userStatusStore.updateStatus(clearRequest);
  }
}

