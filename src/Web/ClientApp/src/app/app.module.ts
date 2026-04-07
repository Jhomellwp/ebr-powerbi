import { APP_ID, NgModule, inject, provideAppInitializer } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LucideAngularModule, Sun, Moon, Laptop, Plus, Settings, MoreHorizontal } from 'lucide-angular';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { AppComponent } from './app.component';
import { ThemeToggleComponent } from './theme-toggle/theme-toggle.component';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { BatchListComponent } from './batches/batch-list.component';
import { BatchDetailsComponent } from './batches/batch-details.component';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { LoginComponent } from 'src/api-authorization/login/login.component';
import { AuthGuard, GuestGuard } from 'src/api-authorization/auth.guard';
import { AuthService } from 'src/api-authorization/auth.service';

@NgModule({
    declarations: [
        AppComponent,
        ThemeToggleComponent,
        LoginComponent,
        BatchListComponent,
        BatchDetailsComponent
    ],
    bootstrap: [AppComponent],
    imports: [
        BrowserModule,
        FormsModule,
        SidebarComponent,
        LucideAngularModule.pick({ Sun, Moon, Laptop, Plus, Settings, MoreHorizontal }),
        RouterModule.forRoot([
            { path: '', redirectTo: 'login', pathMatch: 'full' },
            { path: 'batches', component: BatchListComponent, canActivate: [AuthGuard] },
            { path: 'batches/:id', component: BatchDetailsComponent, canActivate: [AuthGuard] },
            { path: 'login', component: LoginComponent, canActivate: [GuestGuard] }
        ])
    ],
    providers: [
        { provide: APP_ID, useValue: 'ng-cli-universal' },
        { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
        provideAppInitializer(() => inject(AuthService).initialize()),
        provideHttpClient(withInterceptorsFromDi())
    ]
})
export class AppModule { }
