import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'logout']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => httpMock.verify());

  it('logs the user out and redirects to /login on a 401 while a session exists', () => {
    authServiceSpy.isLoggedIn.and.returnValue(true);
    spyOn(router, 'navigate');

    httpClient.get('/api/books').subscribe({ error: () => {} });

    httpMock.expectOne('/api/books').flush({ message: 'Sessionen har gått ut.' }, { status: 401, statusText: 'Unauthorized' });

    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { sessionExpired: true } });
  });

  it('does not log out on a 401 when there was no active session (e.g. a failed login attempt)', () => {
    authServiceSpy.isLoggedIn.and.returnValue(false);
    spyOn(router, 'navigate');

    httpClient.post('/api/auth/login', {}).subscribe({ error: () => {} });

    httpMock
      .expectOne('/api/auth/login')
      .flush({ message: 'Fel e-postadress eller lösenord.' }, { status: 401, statusText: 'Unauthorized' });

    expect(authServiceSpy.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('leaves other error statuses untouched', () => {
    authServiceSpy.isLoggedIn.and.returnValue(true);
    spyOn(router, 'navigate');

    httpClient.get('/api/books').subscribe({ error: () => {} });

    httpMock.expectOne('/api/books').flush({ message: 'Fel' }, { status: 500, statusText: 'Server Error' });

    expect(authServiceSpy.logout).not.toHaveBeenCalled();
  });
});
