import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, map, startWith } from 'rxjs/operators';
import { combineLatest, Observable } from 'rxjs';
import { AuthService } from 'src/api-authorization/auth.service';

@Component({
  standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  showSidebar$: Observable<boolean>;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    const url$ = this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(e => e.urlAfterRedirects),
      startWith(this.router.url)
    );
    this.showSidebar$ = combineLatest([this.authService.isAuthenticated$, url$]).pipe(
      map(([isAuth, url]) => isAuth && !url.split('?')[0].startsWith('/login'))
    );
  }
}
