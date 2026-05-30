import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'ds_token';
  private _token = signal<string | null>(localStorage.getItem(this.TOKEN_KEY));

  readonly isAuthenticated = computed(() => !!this._token());
  readonly token = this._token.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  register(email: string, password: string) {
    return this.http.post<{ id: string }>(
      `${environment.apiUrl}/users/register`,
      { email, password }
    );
  }

  login(email: string, password: string) {
    return this.http.post<{ token: string }>(
      `${environment.apiUrl}/users/login`,
      { email, password }
    ).pipe(
      tap(res => {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        this._token.set(res.token);
      })
    );
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    this._token.set(null);
    this.router.navigate(['/auth/login']);
  }
}
