import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { from, Observable } from 'rxjs';
import { tap, take, map, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(_route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return from(this.authService.initialize()).pipe(
      switchMap(() => this.authService.isAuthenticated$.pipe(take(1))),
      tap(isAuthenticated => {
        if (!isAuthenticated) {
          this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
        }
      }),
      map(isAuthenticated => !!isAuthenticated)
    );
  }
}

/** Use on `/login`: if already authenticated, send user to the app home. */
@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> {
    return from(this.authService.initialize()).pipe(
      switchMap(() => this.authService.isAuthenticated$.pipe(take(1))),
      tap(isAuthenticated => {
        if (isAuthenticated) {
          void this.router.navigate(['/batches']);
        }
      }),
      map(isAuthenticated => !isAuthenticated)
    );
  }
}