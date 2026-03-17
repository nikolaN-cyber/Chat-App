import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { authStore } from '../../shared/store/auth.store';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

  private fb = inject(FormBuilder);
  private authStore = inject(authStore)

  registerForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(8)]],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    age: [0, [Validators.required, Validators.min(12)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  onSubmit() {
    console.log("Metoda okinuta")
    if (this.registerForm.valid) {
      const data = this.registerForm.getRawValue();
      this.authStore.register(data);
    } else {
      console.log('error');
    }
  }
}
