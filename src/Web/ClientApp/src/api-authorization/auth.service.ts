import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _isAuthenticated = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this._isAuthenticated.asObservable();

  constructor(private http: HttpClient) {}

  initialize(): Observable<boolean> {
    return this.http.get<{ isAuthenticated: boolean }>('/api/Users/info').pipe(
      map(response => !!response?.isAuthenticated),
      catchError(() => of(false)),
      tap(isAuth => this._isAuthenticated.next(isAuth))
    );
  }

  login(email: string, password: string): Observable<void> {
    return this.http.post<void>('/api/Users/login', { email, password }).pipe(
      tap(() => this._isAuthenticated.next(true)),
      map(() => void 0)
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/Users/logout', {}).pipe(
      tap(() => this._isAuthenticated.next(false))
    );
  }
}