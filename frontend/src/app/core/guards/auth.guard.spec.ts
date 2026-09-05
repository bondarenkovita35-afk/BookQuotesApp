import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn']);

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }],
    });

    router = TestBed.inject(Router);
  });

  function runGuard(url: string) {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as never, { url } as never),
    );
  }

  it('allows navigation when the user is logged in', () => {
    authServiceSpy.isLoggedIn.and.returnValue(true);

    expect(runGuard('/books')).toBeTrue();
  });

  it('redirects to /login with a returnUrl when the user is not logged in', () => {
    authServiceSpy.isLoggedIn.and.returnValue(false);

    const result = runGuard('/books') as UrlTree;

    expect(result instanceof UrlTree).toBeTrue();
    expect(router.serializeUrl(result)).toBe('/login?returnUrl=%2Fbooks');
  });
});
