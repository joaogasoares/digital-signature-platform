import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="auth-container">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>Criar conta</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>E-mail</mat-label>
              <input matInput type="email" [(ngModel)]="email" name="email" required />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Senha</mat-label>
              <input matInput type="password" [(ngModel)]="password" name="password" required />
              <mat-hint>Mínimo 8 caracteres, maiúscula, minúscula e número</mat-hint>
            </mat-form-field>
            @if (error()) {
              <p class="error-msg">{{ error() }}</p>
            }
            @if (success()) {
              <p class="success-msg">Conta criada! <a routerLink="/auth/login">Faça login</a></p>
            }
            <button mat-raised-button color="primary" type="submit" class="full-width" [disabled]="loading()">
              @if (loading()) { <mat-spinner diameter="20"></mat-spinner> } @else { Registrar }
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions>
          <a routerLink="/auth/login">Já tem conta? Entrar</a>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .auth-container { display:flex; justify-content:center; align-items:center; height:100vh; background:#f5f5f5; }
    .auth-card { width:100%; max-width:400px; padding:16px; }
    .full-width { width:100%; margin-bottom:12px; }
    .error-msg { color:#f44336; font-size:14px; }
    .success-msg { color:#4caf50; font-size:14px; }
  `]
})
export class RegisterComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    this.loading.set(true);
    this.error.set(null);

    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        this.auth.login(this.email, this.password).subscribe({
          next: () => this.router.navigate(['/documents']),
          error: () => { this.success.set(true); this.loading.set(false); }
        });
      },
      error: (err) => {
        this.error.set(err.error?.error ?? 'Erro ao criar conta.');
        this.loading.set(false);
      }
    });
  }
}
