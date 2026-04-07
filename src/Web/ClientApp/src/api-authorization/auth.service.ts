import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, firstValueFrom, of, throwError } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';

/** Matches API JSON (`CurrentUserInfoVm`, camelCase). */
export interface CurrentUserInfoDto {
  isAuthenticated: boolean;
  userId: string | null;
  email: string | null;
  userName: string | null;
  roles: string[];
}

/** Subset exposed for UI when authenticated. */
export interface AuthUser {
  userId: string | null;
  email: string | null;
  userName: string | null;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _isAuthenticated = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this._isAuthenticated.asObservable();

  private _user = new BehaviorSubject<AuthUser | null>(null);
  user$ = this._user.asObservable();

  private initPromise: Promise<void> | null = null;

  constructor(private http: HttpClient) {}

  /**
   * Ensures `/api/Users/info` has run at least once. Reused across guards and APP_INITIALIZER so startup does one round-trip.
   * Cleared on logout or 401 so the next call refetches.
   */
  initialize(): Promise<void> {
    if (this.initPromise) {
      return this.initPromise;
    }
    this.initPromise = firstValueFrom(
      this.getUserInfo().pipe(
        map(() => void 0),
        catchError(() => {
          this.applyUserInfo(this.unauthenticatedDto());
          return of(undefined);
        })
      )
    );
    return this.initPromise;
  }

  /** Server rejected the session (401) or client logged out; drop cached bootstrap so guards refetch `/info`. */
  notifySessionInvalid(): void {
    this.initPromise = null;
    this._isAuthenticated.next(false);
    this._user.next(null);
  }

  login(email: string, password: string): Observable<void> {
    return this.http.post<void>('/api/Users/login', { email, password }).pipe(
      switchMap(() => this.getUserInfo()),
      switchMap(() => {
        if (!this._isAuthenticated.value) {
          return throwError(
            () =>
              new Error(
                'Session was not established after login. Use `ng serve` against the API in Development, or open the SPA from the same URL as the backend.'
              )
          );
        }
        // Force guards / APP_INITIALIZER to refetch `/info` on next navigation instead of reusing the anonymous bootstrap.
        this.initPromise = null;
        return of(undefined);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/Users/logout', {}).pipe(
      tap(() => this.notifySessionInvalid())
    );
  }

  private getUserInfo(): Observable<CurrentUserInfoDto> {
    return this.http.get<CurrentUserInfoDto>('/api/Users/info').pipe(tap(vm => this.applyUserInfo(vm)));
  }

  private unauthenticatedDto(): CurrentUserInfoDto {
    return {
      isAuthenticated: false,
      userId: null,
      email: null,
      userName: null,
      roles: []
    };
  }

  private applyUserInfo(vm: CurrentUserInfoDto): void {
    if (!vm?.isAuthenticated) {
      this._isAuthenticated.next(false);
      this._user.next(null);
      return;
    }
    this._isAuthenticated.next(true);
    this._user.next({
      userId: vm.userId ?? null,
      email: vm.email ?? null,
      userName: vm.userName ?? null,
      roles: vm.roles ? [...vm.roles] : []
    });
  }
}
