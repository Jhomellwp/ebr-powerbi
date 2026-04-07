import { Injectable, Injector } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthorizeInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private injector: Injector
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authReq = req.clone({ withCredentials: true });
    return next.handle(authReq).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse
          && error.status === 401
          && !error.url?.includes('/manage/info')
          && !error.url?.includes('/api/Users/login')
          && !this.router.url.startsWith('/login')) {
          // Lazy inject avoids constructing AuthService before HttpClient is ready.
          this.injector.get(AuthService).notifySessionInvalid();
          void this.router.navigate(['/login'], { queryParams: { returnUrl: window.location.pathname + window.location.search } });
        }
        return throwError(() => error);
      })
    );
  }
}