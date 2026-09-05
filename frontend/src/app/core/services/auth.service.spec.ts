import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('starts logged out when there is no stored session', () => {
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('stores the token and email after a successful login', () => {
    service.login({ email: 'test@example.com', password: 'Password123' }).subscribe();

    const req = httpMock.expectOne((r) => r.url.endsWith('/auth/login'));
    req.flush({ token: 'abc123', expiresAtUtc: new Date().toISOString(), email: 'test@example.com' });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getToken()).toBe('abc123');
    expect(service.currentUserEmail()).toBe('test@example.com');
  });

  it('stores the session after a successful registration', () => {
    service.register({ email: 'new@example.com', password: 'Password123' }).subscribe();

    const req = httpMock.expectOne((r) => r.url.endsWith('/auth/register'));
    req.flush({ token: 'xyz789', expiresAtUtc: new Date().toISOString(), email: 'new@example.com' });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.currentUserEmail()).toBe('new@example.com');
  });

  it('clears the session on logout', () => {
    service.login({ email: 'test@example.com', password: 'Password123' }).subscribe();
    httpMock.expectOne((r) => r.url.endsWith('/auth/login')).flush({
      token: 'abc123',
      expiresAtUtc: new Date().toISOString(),
      email: 'test@example.com',
    });

    service.logout();

    expect(service.isLoggedIn()).toBeFalse();
    expect(service.currentUserEmail()).toBeNull();
  });
});
