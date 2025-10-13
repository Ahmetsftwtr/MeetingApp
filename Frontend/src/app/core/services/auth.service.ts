import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map } from 'rxjs';
import { Router } from '@angular/router';
import { User } from '../models/auth/Interfaces/User';
import { LoginResponse, RegisterResponse } from '../models/auth/Response/authResponse.dto';
import { ApiResponse } from '../models/base/api-response.model';
import { LoginRequestDto } from '../models/auth/Request/loginRequest.dto';
import { RegisterRequestDto } from '../models/auth/Request/registerRequest.dto';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  
  private apiUrl = `${environment.apiUrl}/Auth`;
  
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();
  isAuthenticated$ = this.currentUser$.pipe(map(user => !!user));
  
  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem('user');
    return userJson ? JSON.parse(userJson) : null;
  }

  login(request: LoginRequestDto): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, request)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            localStorage.setItem('token', response.message);
            const user: User = {
              id: response.data.id,
              firstName: response.data.firstName,
              lastName: response.data.lastName,
              email: response.data.email,
              phone: response.data.phone,
              profileImagePath: response.data.profileImagePath
            };
            localStorage.setItem('user', JSON.stringify(user));
            this.currentUserSubject.next(user);
          }
        })
      );
  }

  register(request: RegisterRequestDto): Observable<ApiResponse<RegisterResponse>> {
    const formData = new FormData();
    formData.append('firstName', request.firstName);
    formData.append('lastName', request.lastName);
    formData.append('email', request.email);
    formData.append('phone', request.phone);
    formData.append('password', request.password);
    
    if (request.profileImage) {
      formData.append('profileImage', request.profileImage);
    }

    return this.http.post<ApiResponse<RegisterResponse>>(`${this.apiUrl}/register`, formData)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data && response.message) {
            localStorage.setItem('token', response.message);
            const user: User = {
              id: response.data.id,
              firstName: response.data.firstName,
              lastName: response.data.lastName,
              email: response.data.email,
              phone: response.data.phone,
              profileImagePath: response.data.profileImagePath
            };
            localStorage.setItem('user', JSON.stringify(user));
            this.currentUserSubject.next(user);
          }
        })
      );
  }

  getProfile(): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/me`);
  }

  private fetchAndSetUserProfile(): void {
    this.getProfile().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          localStorage.setItem('user', JSON.stringify(response.data));
          this.currentUserSubject.next(response.data);
        }
      },
      error: () => {
        this.logout();
      }
    });
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getProfileImageUrl(path?: string): string {
    if (!path) return 'assets/images/default-avatar.png';
    return `${environment.apiUrl}/${path}`;
  }
}