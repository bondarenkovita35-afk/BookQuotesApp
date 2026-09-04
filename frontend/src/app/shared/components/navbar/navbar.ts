import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
})
export class Navbar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);

  readonly currentUserEmail = this.authService.currentUserEmail;
  readonly theme = this.themeService.theme;

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }
}
