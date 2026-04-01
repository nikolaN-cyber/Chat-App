import { Component, inject } from '@angular/core';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import { MatFormField } from "@angular/material/input";
import { MatLabel } from '@angular/material/input';
import { MatInput } from '@angular/material/input';
import { MatAnchor } from "@angular/material/button";
import { authStore } from '../../store/auth.store';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { FormBuilder } from '@angular/forms';
import { EditUser } from '../../../core/models/user';

@Component({
  selector: 'app-edit-profile-dialog',
  imports: [MatDialogModule, MatFormField, MatLabel, MatInput, MatAnchor, ReactiveFormsModule],
  templateUrl: './edit-profile-dialog.html',
  styleUrl: './edit-profile-dialog.css',
})
export class EditProfileDialog {

  dialog = inject(MatDialog);
  private fb = inject(FormBuilder);
  readonly authStore = inject(authStore);
    
  editProfileForm = this.fb.nonNullable.group({
    username: [this.authStore.currentUser()?.username, [Validators.required]],
    firstName: [this.authStore.currentUser()?.firstName, [Validators.required]],
    lastName: [this.authStore.currentUser()?.lastName, [Validators.required]],
    age: [this.authStore.currentUser()?.age, [Validators.required]]
  })

  onSubmit(){
    if (this.editProfileForm.valid){
      const data = this.editProfileForm.getRawValue() as EditUser;
      this.authStore.editProfile(data);
      this.dialog.closeAll();
    }
  }

  onCloseClicked() {
    this.dialog.closeAll();
  }
}
