import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { authStore } from '../../shared/store/auth.store';
import { MatCard } from "@angular/material/card";
import { MatCardHeader } from '@angular/material/card';
import { MatCardTitle } from '@angular/material/card';
import { MatFormField } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';



@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, MatCard, MatCardHeader, MatCardTitle, MatFormField, MatFormFieldModule, MatInputModule, MatIconModule, RouterModule, MatCardModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private fb = inject(FormBuilder);
  private authStore = inject(authStore)

  loginForm = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  onSubmit(){
    if (this.loginForm.valid){
      const credentials = this.loginForm.getRawValue();
      this.authStore.login(credentials);
    }
  }
}
