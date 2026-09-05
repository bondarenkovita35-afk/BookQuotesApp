import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { guestGuard } from './guest.guard';

describe('guestGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn']);

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }],
    });

    router = TestBed.inject(Router);
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
  }

  it('allows navigation when the user is not logged in', () => {
    authServiceSpy.isLoggedIn.and.returnValue(false);

    expect(runGuard()).toBeTrue();
  });

  it('redirects to /books when the user is already logged in', () => {
    authServiceSpy.isLoggedIn.and.returnValue(true);

    const result = runGuard() as UrlTree;

    expect(result instanceof UrlTree).toBeTrue();
    expect(router.serializeUrl(result)).toBe('/books');
  });
});
