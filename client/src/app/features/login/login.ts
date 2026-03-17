import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { authStore } from '../../shared/store/auth.store';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private fb = inject(FormBuilder);
  private authStore = inject(authStore)

  loginForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(8)]],
    password: ['', [Validators.required]]
  });

  onSubmit(){
    if (this.loginForm.valid){
      const credentials = this.loginForm.getRawValue();
      this.authStore.login(credentials);
    }
  }
}
