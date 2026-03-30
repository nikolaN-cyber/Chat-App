import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { authStore } from '../../shared/store/auth.store';
import { MatCard } from "@angular/material/card";
import { MatCardContent } from '@angular/material/card';
import { MatCardActions } from '@angular/material/card';
import { MatFormField } from '@angular/material/input';
import { MatLabel } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatCard,
    MatCardActions,
    MatCardContent,
    MatFormField,
    MatLabel,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    RouterModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

  private fb = inject(FormBuilder);
  private authStore = inject(authStore)

  registerForm = this.fb.nonNullable.group({
    username: [''],
    firstName: [''],
    lastName: [''],
    age: [0],
    email: [''],
    password: [''],
    confirmPassword: ['']
  });

  onSubmit() {
    if (this.registerForm.valid) {
      const data = this.registerForm.getRawValue();
      this.authStore.register(data);
    }
  }
}
