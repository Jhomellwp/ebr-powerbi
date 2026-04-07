import { Component, computed, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from 'src/api-authorization/auth.service';

@Component({
  standalone: true,
  selector: 'app-sidebar',
  imports: [RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  collapsed = signal(false);

  readonly user = toSignal(this.authService.user$, { initialValue: null });

  /** Primary line: friendly name or email. */
  readonly userDisplay = computed(() => {
    const u = this.user();
    if (!u) {
      return '';
    }
    return (u.userName?.trim() || u.email?.trim() || u.userId || '').trim();
  });

  /** Secondary line when both name and email exist and differ. */
  readonly userSubtitle = computed(() => {
    const u = this.user();
    if (!u) {
      return '';
    }
    const name = u.userName?.trim() ?? '';
    const email = u.email?.trim() ?? '';
    if (name && email && name !== email) {
      return email;
    }
    return '';
  });

  readonly userTooltip = computed(() => {
    const u = this.user();
    if (!u) {
      return '';
    }
    const name = u.userName?.trim();
    const email = u.email?.trim();
    if (name && email) {
      return `${name} — ${email}`;
    }
    return name || email || u.userId || '';
  });

  readonly userInitials = computed(() => {
    const u = this.user();
    if (!u) {
      return '';
    }
    const raw = (u.userName || u.email || '').trim();
    if (!raw) {
      return '?';
    }
    const parts = raw.split(/\s+/).filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase().slice(0, 2);
    }
    return raw.slice(0, 2).toUpperCase();
  });

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  toggle(): void {
    this.collapsed.update(c => !c);
  }

  logout(event: Event): void {
    event.preventDefault();
    this.authService.logout().subscribe({
      next: () => void this.router.navigate(['/login'])
    });
  }
}
